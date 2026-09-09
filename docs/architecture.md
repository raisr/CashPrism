# Architecture

The projects that exist today, how the solution is laid out, and why it is cut
this way. The binding rules — the dependency direction, who may reference an
Infrastructure project, the test naming scheme — live in
[`Agents.md`](../Agents.md#architecture); this document only describes the
current state.

CashPrism follows the onion architecture: the dependency arrow always points
inwards, never outwards. `Domain` sits at the centre and knows nothing about the
outside world; every layer around it may depend only on layers closer to the
centre.

## Projects

| Project | Role | May depend on |
|---|---|---|
| `src/CashPrism.Domain` | Models and business rules | nothing — no EF Core, no ASP.NET, no NuGet beyond the BCL |
| `src/CashPrism.Application` | Use cases, orchestration, DTOs, **interfaces** for anything external | Domain |
| `src/CashPrism.Infrastructure` | Implements the Application interfaces: DbContext, repositories, file system, clock | Application, Domain |
| `src/CashPrism.Infrastructure.Finanzguru` | ClosedXML parser for the FinanzGuru xlsx. Keeps the ClosedXML dependency out of everything else | Application, Domain |
| `src/CashPrism.Web` | Razor Class Library: Blazor components, routing, auth UI, endpoint mapping. Exposes `AddCashPrismWeb()` / `MapCashPrismWeb()` | Application, Domain |
| `src/CashPrism.Shell` | The executable and the composition root: Kestrel setup, port and binding, startup migrations, LAN URL, browser launch, single-instance guard | everything |
| `src/CashPrism.Anonymiser` | The second composition root: a standalone console tool that turns a real FinanzGuru export into one safe to share | Application, Domain, Infrastructure.Finanzguru |

`Shell` and `Anonymiser` are the only two projects that reference an
Infrastructure project — both are composition roots, so both are allowed to wire
concrete infrastructure to a use case.

## Test projects

| Project | Covers |
|---|---|
| `src/Tests/CashPrism.Architecture.Tests` | Solution-wide rules: which project may reference which |
| `src/Tests/CashPrism.Infrastructure.Finanzguru.Tests.Unit` | `Infrastructure.Finanzguru` in isolation, without a workbook |
| `src/Tests/CashPrism.Shell.Tests.Integration` | `Shell` end to end, hosting included |
| `src/Tests/CashPrism.Anonymiser.Tests.Unit` | Command-line parsing, no file on disk |
| `src/Tests/CashPrism.Anonymiser.Tests.Integration` | The file round trip, built in code — no binary fixture in the repository |

## Directory layout

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

The solution file is `src/CashPrism.slnx`, so `src/Tests` is a plain folder
inside the solution root, not a solution folder that has to be kept in sync.

## Why it is cut this way

- **`Domain` has no framework in it.** Keeping EF Core and ASP.NET attributes out
  of the models means the rules can be read, and tested, without a database or a
  web server. Persistence details live in configurations under
  `Infrastructure`, mapped onto plain models.
- **`Application` owns the interfaces, `Infrastructure` implements them.** When a
  use case needs the database or the file system it declares what it needs;
  `Infrastructure` depends inwards to satisfy it. `Infrastructure` is never
  referenced from `Application` or `Domain`, so the dependency arrow cannot turn
  around.
- **A second import source is a new project, not an edit.** Each parser gets its
  own `CashPrism.Infrastructure.<Name>` project so a heavy dependency like
  ClosedXML stays contained. Adding FinanzGuru's successor does not touch the
  FinanzGuru parser.
- **`Web` is a library, not a host.** It has no `Program.cs`, reads no
  configuration and never references an Infrastructure project. Everything that
  only makes sense once the process is running — Kestrel, the port, migrations,
  the browser launch — belongs in `Shell`. That split is what lets the
  integration tests host `Web` without `Shell`.
- **`Shell` is wiring, not logic.** It is the one place a newcomer reads first,
  so it stays a composition root: no business rules, no UI. If something in
  `Shell` gets interesting enough to test, it is in the wrong project.
- **`Anonymiser` is a development tool, not part of the shipped application.** It
  is a second composition root with its own entry point, and it never references
  ClosedXML — an `.xlsx` is a zip it takes apart and puts back together itself,
  so a real FinanzGuru export stays recognisable as one.
- **`CashPrism.Architecture.Tests` enforces the reference graph.** The rules
  above are not a gentleman's agreement: the build fails if a project outside
  `Shell` and `Anonymiser` takes a dependency on an Infrastructure project.
