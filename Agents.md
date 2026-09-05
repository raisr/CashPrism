# AGENTS.md

## Overview

CashPrism is a desktop application that reads data exports from the personal finance app "FinanzGuru" and makes your own finances analysable on a large screen.

## Tech stack

- tbd

## Architecture

**Onion architecture. The dependency arrow always points inwards, never outwards.**

| Project | Role | May depend on |
|---|---|---|
| `src/CashPrism.Domain` | Entities, value objects, domain events, business rules | nothing — no EF Core, no ASP.NET, no NuGet beyond the BCL |
| `src/CashPrism.Application` | Use cases, orchestration, DTOs, **interfaces** for anything external | Domain |
| `src/CashPrism.Infrastructure` | Implements the Application interfaces: DbContext, repositories, HTTP clients, file system, clock | Application, Domain |
| `src/CashPrism.Web` | Entry point: endpoints/controllers, DI wiring, configuration, mapping | Application, Domain, and Infrastructure **only** in `Program.cs` for registration |
| `src/tests/CashPrism.Tests.Unit` | Domain and Application | — |
| `src/tests/CashPrism.Tests.Integration` | Web and Infrastructure end to end | — |

Consequences worth stating, because they are where it usually goes wrong:

- Infrastructure never gets referenced from Application or Domain. If Application needs the database, it declares an interface and Infrastructure implements it
- Domain contains no attributes from EF Core or ASP.NET. Persistence details live in configurations under Infrastructure
- Nothing outside Domain decides what is valid. Guard clauses belong in the entity, not in the controller

## Code style

- `.editorconfig` in the repository root is binding and overrules any differing opinion
- Keep `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` switched on
- Everything written into this repository is **English**: identifiers, comments, XML docs, commit messages, documentation
- File-scoped namespaces, primary constructors for injection, records for DTOs and commands
- `async`/`await` end to end, pass the `CancellationToken` through — no `.Result`, no `.Wait()`, no `async void` (except event handlers)
- Constructor dependency injection, no service locator, no `new` on services
- Log through `ILogger<T>` with structured templates (`logger.LogInformation("Order {OrderId} shipped", id)`) — no `Console.WriteLine`, no string interpolation inside the log template
- Configuration through `IOptions<T>`, no magic strings scattered around

### Patterns we do not use — never suggest them

- <Exceptions as control flow for expected business outcomes — use a result type>
- <Static helpers holding state, service locator, `DateTime.Now` in domain code>

## Tests

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

- Test file mirrors the source path: `src/CashPrism.Application/Orders/OrderService.cs` → `test/CashPrism.UnitTests/Application/Orders/OrderServiceTests.cs`
- Method name reads `Scenario_ExpectedResult` — the method under test is already the nested class, do not repeat it
- Arrange/Act/Assert, one behaviour per test, no logic in the test itself
- New or changed logic without a test counts as unfinished, even when nobody asked for one
- Integration tests run against <Testcontainers | LocalDB>, never against a shared database

## Security

- No secrets in the repository. User Secrets locally, <Key Vault | environment variables> in production
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
- If `AGENTS.local.md` exists next to this file, read it as well and let it win on conflicts. It carries personal and machine-specific settings, is git-ignored, and is never required for anyone else to work on this repository

## Domain terms

| Term | Means |
|---|---|

## Signing AI-generated content

When an agent creates content in **external** systems (Trello, GitHub issues, PRs, wikis, comments), it must be recognisable as not typed by a human. Append the signature, separated by `---`:

```markdown
---
🤖 *Claude was here. No hands, but opinions.*
*<YYYY-MM-DD>*
```

The wording is fixed — do not rephrase it per context, or it stops being reliably searchable. Search term: **`Claude was here`**. This does not apply to files inside the repository; there the Git history is the provenance.
