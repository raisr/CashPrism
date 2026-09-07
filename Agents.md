# Agents.md

## Overview

CashPrism reads data exports from the personal finance app "FinanzGuru" and makes your own finances analysable on a large screen.

Imports are additive: every import adds rows, it never overwrites. Each row gets a fingerprint (a hash over date, amount, currency, account, counterparty and payment reference); a fingerprint that is already known is skipped. The raw contents of every imported file are stored unchanged as well.

One machine hosts the application, every other device on the home network reaches it through a browser. Everything stays local: no cloud, no external account.

See `README.md` for the user-facing description.

## Tech stack

| Area | Choice |
|---|---|
| Runtime | .NET 10 (LTS) |
| UI | Blazor Web App, render mode `InteractiveServer` |
| Web server | Kestrel, built in |
| Database | SQLite via EF Core, `./data/cashprism.db` |
| Excel | ClosedXML |
| Auth | One shared password, PBKDF2, cookie authentication |
| Delivery | Self-contained single-file binary per platform |

The database lives on a local disk only, never on a network share. SQLite locking over SMB is unreliable. This is a decision, not an oversight — do not propose moving it.

**We do not use DDD.** No value objects, no domain events, no aggregates. `Domain` holds plain models and the rules that operate on them.

## Architecture

**Onion architecture. The dependency arrow always points inwards, never outwards.**

| Project | Role | May depend on |
|---|---|---|
| `src/CashPrism.Domain` | Models and business rules | nothing — no EF Core, no ASP.NET, no NuGet beyond the BCL |
| `src/CashPrism.Application` | Use cases, orchestration, DTOs, **interfaces** for anything external | Domain |
| `src/CashPrism.Infrastructure` | Implements the Application interfaces: DbContext, repositories, file system, clock | Application, Domain |
| `src/CashPrism.Infrastructure.Finanzguru` | ClosedXML parser for the FinanzGuru xlsx. Keeps the ClosedXML dependency out of everything else | Application, Domain |
| `src/CashPrism.Web` | Razor Class Library: Blazor components, routing, auth UI, endpoint mapping. Exposes `AddCashPrismWeb()` / `MapCashPrismWeb()` | Application, Domain |
| `src/CashPrism.Shell` | The executable and the composition root: Kestrel setup, port and binding, startup migrations, LAN URL, browser launch, single-instance guard | everything |
| `src/CashPrism.Anonymiser` | The second composition root: a standalone console tool that turns a real FinanzGuru export into one safe to share | Application, Domain, Infrastructure.Finanzguru |
| `src/Tests/CashPrism.Architecture.Tests` | Solution-wide rules: which project may reference which | — |
| `src/Tests/CashPrism.Infrastructure.Finanzguru.Tests.Unit` | `Infrastructure.Finanzguru` in isolation, without a workbook | — |
| `src/Tests/CashPrism.Shell.Tests.Integration` | `Shell` end to end, hosting included | — |
| `src/Tests/CashPrism.Anonymiser.Tests.Unit` | Command-line parsing, no file on disk | — |
| `src/Tests/CashPrism.Anonymiser.Tests.Integration` | The file round trip, built in code — no binary fixture in the repository | — |

`Shell` and `Anonymiser` are the only two projects allowed to reference an Infrastructure project — both are composition roots, so both are allowed to wire concrete infrastructure to a use case. `CashPrism.Architecture.Tests` fails the build if a project outside that set takes such a dependency.

The solution file is `src/CashPrism.slnx`, so `src/Tests` is a plain folder inside the solution root.

```
src/
  CashPrism.slnx
  CashPrism.Domain/
  CashPrism.Application/
  CashPrism.Infrastructure/
  CashPrism.Infrastructure.Finanzguru/
  CashPrism.Web/
  CashPrism.Shell/
  CashPrism.Anonymiser/
  Tests/
    CashPrism.Architecture.Tests/
    CashPrism.Infrastructure.Finanzguru.Tests.Unit/
    CashPrism.Shell.Tests.Integration/
    CashPrism.Anonymiser.Tests.Unit/
    CashPrism.Anonymiser.Tests.Integration/
```

Consequences worth stating, because they are where it usually goes wrong:

- Infrastructure never gets referenced from Application or Domain. If Application needs the database, it declares an interface and Infrastructure implements it
- Domain contains no attributes from EF Core or ASP.NET. Persistence details live in configurations under Infrastructure
- Nothing outside Domain decides what is valid. Guard clauses belong in the model, not in the controller
- A second import source becomes a new `CashPrism.Infrastructure.<Name>` project next to the existing one, not a change to the existing parser
- `Web` is a library, not a host. It never references an Infrastructure project, never reads configuration and has no `Program.cs`. Everything that only makes sense once the process is running belongs in `Shell`
- `Shell` contains no business logic and no UI. If something there gets interesting enough to test, it is in the wrong project
- `Anonymiser` is a development tool, not part of the shipped application. It never references ClosedXML — an `.xlsx` is a zip it takes apart and puts back together itself, so a real FinanzGuru export stays recognisable as one

## Code style

- `.editorconfig` in the repository root is binding and overrules any differing opinion
- Keep `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` switched on
- Everything written into this repository is **English**: identifiers, comments, XML docs, commit messages, documentation
- File-scoped namespaces, primary constructors for injection, records for DTOs and commands
- **No top-level statements.** The entry point is an ordinary `Program` class with an
  explicit `Main`, in a namespace like every other type. The composition root is the one
  place a newcomer reads first — it deserves a name, an XML doc and a file that looks like
  the rest of the codebase
- **The namespace always follows the directory structure.** A file in
  `src/CashPrism.Web/Configuration/` is in `CashPrism.Web.Configuration`, with no
  exception for extension classes or anything else. `dotnet format` enforces it
  (`IDE0130`), so a mismatch fails the gate rather than surviving in review
- `async`/`await` end to end, pass the `CancellationToken` through — no `.Result`, no `.Wait()`, no `async void` (except event handlers)
- Constructor dependency injection, no service locator, no `new` on services
- Log through `ILogger<T>` with structured templates (`logger.LogInformation("Order {OrderId} shipped", id)`) — no `Console.WriteLine`, no string interpolation inside the log template. The startup banner in `Shell` is the one allowed exception: it is user-facing output, not logging
- Configuration through `IOptions<T>`, no magic strings scattered around

### Patterns we do not use — never suggest them

- Exceptions as control flow for expected business outcomes — use a result type
- Static helpers holding state, service locator, `DateTime.Now` in domain code
- DDD building blocks: value objects, domain events, aggregate roots, repositories-per-aggregate

## Hosting

CashPrism is shipped as one executable that a person double-clicks, so `Shell` carries the concerns a web project normally does not have:

- Bind Kestrel to `0.0.0.0` on a fixed, overridable port. Binding to localhost only would lock out every other device
- Create `./data` if missing and apply EF Core migrations before serving the first request
- Print the reachable LAN URL to the console so it can be typed on a phone
- Open the local browser on start, unless started with a switch that says otherwise
- Refuse to start a second instance against the same database file

`Web` must stay hostable without `Shell` — that is what the integration tests use.

## Tests

**A test project belongs to exactly one production project and is named
`<Project>.Tests.Unit` or `<Project>.Tests.Integration`.** It lives under
`src/Tests/`, and folder, `.csproj`, assembly name and root namespace all carry
that same name. A project only gets a test project once it actually has tests —
no empty projects on stock.

The one exception is **`CashPrism.Architecture.Tests`**: tests that assert
solution-wide rules, such as which project may reference which. They belong to
no single project, so they carry no `.Unit`/`.Integration` suffix. Do not add a
second exception without a ticket that argues for it.

**Structure: one test class per class under test, one nested class per method under test.**

```csharp
public sealed class OrderServiceTests            // class under test
{
    public sealed class PlaceOrderAsync          // method under test
    {
        [Fact]
        public async Task Returns_Failure_When_Cart_Is_Empty() { }

        [Fact]
        public async Task Persists_Order_When_Cart_Is_Valid() { }
    }
}
```

- Test file mirrors the source path *inside its project*: `src/CashPrism.Application/Orders/OrderService.cs` → `src/Tests/CashPrism.Application.Tests.Unit/Orders/OrderServiceTests.cs`. The project name already says which project is under test, so it is not repeated as a folder
- Method name reads `Scenario_ExpectedResult` — the method under test is already the nested class, do not repeat it
- Arrange/Act/Assert, one behaviour per test, no logic in the test itself
- New or changed logic without a test counts as unfinished, even when nobody asked for one
- Integration tests run against a throwaway SQLite database per test run, never against a shared one

## Documentation

Three files, three roles. Keep them apart:

| File | Role |
|---|---|
| `README.md` | First contact: what CashPrism is, why it exists, that imports are additive, clone and run, status, licence |
| `docs/` | Explanation and reference: how it works and why it was built that way. [`docs/README.md`](docs/README.md) is the index, one line per document |
| `Agents.md` | Binding rules and conventions. **It prescribes, it does not describe** — explanatory prose belongs in `docs/`, and this file links to it |

**Documentation is part of the change.** When a change makes a document in
`docs/`, the `README.md` or this file wrong, incomplete or misleading, updating it
belongs in the same branch and the same pull request — not into a follow-up
ticket. A pull request that leaves documentation contradicting the code is not
done. Conversely, no documentation is written for something that does not exist
yet.

## Security

- No secrets in the repository. User Secrets locally, environment variables when deployed
- No concatenated SQL — parameterised queries or LINQ only
- Validate input server-side, encode output, use anti-forgery tokens on forms
- Authorise every endpoint explicitly (`[Authorize]` by default, `[AllowAnonymous]` as a justified exception)
- No personal data in logs

## Git & pull / merge requests

- Branches: `feature/<ticket>-<short-description>`, `fix/<ticket>-<short-description>`
- Conventional commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`
- PR description: **what** changed and **why**, plus evidence that tests ran
- `dotnet format`, `dotnet build` and `dotnet test` pass before anything is committed

## Boundaries for agents

- **Never** run a migration or a script against a production database
- No major-version NuGet upgrades without asking
- No new dependencies without justification — check whether the BCL already covers it
- Do not weaken the layer rules to make something compile. If the dependency direction is in the way, the design is wrong, not the rule
- Do not touch generated files

## Working style

- **When something is unclear, ask instead of guessing. No answer beats a bad answer.** This outranks every other rule here
- Keep changes small and reviewable, no unrequested refactoring of surrounding code
- Prefer the boring solution; introduce a pattern only where it earns its complexity
- If `Agents.local.md` exists next to this file, read it as well and let it win on conflicts. It carries personal and machine-specific settings, is git-ignored, and is never required for anyone else to work on this repository

## Domain terms

| Term | Means |
|---|---|
| Import run | One processed export file, recorded with the file hash |
| Raw row | An untouched row from an imported file, stored as JSON |
| Fingerprint | Hash over date, amount, currency, account, counterparty and payment reference. Decides whether two rows are the same booking |
| Transaction | A single booking, deduplicated by its fingerprint |

## Signing AI-generated content

When an agent creates content in **external** systems (Trello, GitHub issues, PRs, wikis, comments), it must be recognisable as not typed by a human. Append the signature, separated by `---`:

```markdown
---
🤖 *Claude was here. No hands, but opinions.*
*<YYYY-MM-DD>*
```

The wording is fixed — do not rephrase it per context, or it stops being reliably searchable. Search term: **`Claude was here`**. This does not apply to files inside the repository; there the Git history is the provenance.
