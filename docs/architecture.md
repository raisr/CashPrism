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
| `src/CashPrism.Infrastructure` | Implements the Application interfaces: DbContext, the import store, migrations, clock | Application, Domain |
| `src/CashPrism.Infrastructure.Finanzguru` | Reads the Finanzguru xlsx with ClosedXML and projects its rows onto bookings. Keeps both the ClosedXML dependency and the German column names out of everything else | Application, Domain |
| `src/CashPrism.Web` | Razor Class Library: Blazor components, routing, auth UI, endpoint mapping. Exposes `AddCashPrismWeb()` / `MapCashPrismWeb()` | Application, Domain |
| `src/CashPrism.Shell` | The executable and the composition root: Kestrel setup, port and binding, startup migrations, LAN URL, browser launch, single-instance guard | everything |
| `src/Tools/CashPrism.Anonymiser` | The second composition root: a standalone console tool that turns a real Finanzguru export into one safe to share | Application, Domain, Infrastructure.Finanzguru |
| `src/Tools/CashPrism.DemoData` | The third composition root: a standalone console tool that writes a fully synthetic Finanzguru export to try CashPrism on | Infrastructure.Finanzguru |

`Shell`, `Anonymiser` and `DemoData` are the only projects that reference an
Infrastructure project — all three are composition roots, so all three are
allowed to wire concrete infrastructure to a use case.

## Test projects

| Project | Covers |
|---|---|
| `src/Tests/CashPrism.Architecture.Tests` | Solution-wide rules: which project may reference which |
| `src/Tests/CashPrism.Application.Tests.Unit` | The use cases against test doubles: what an import inserts, updates, leaves alone and refuses |
| `src/Tests/CashPrism.Domain.Tests.Unit` | The models and the rules on them: identity, split roles, which import run is the later one |
| `src/Tests/CashPrism.Infrastructure.Finanzguru.Tests.Unit` | `Infrastructure.Finanzguru` in isolation, without a workbook |
| `src/Tests/CashPrism.Infrastructure.Finanzguru.Tests.Integration` | The export reader and the import source against a workbook built in code — no binary fixture in the repository |
| `src/Tests/CashPrism.Infrastructure.Tests.Integration` | The schema and the import store against a throwaway SQLite file: round trips, keys, cascade, and that amounts are stored as cents |
| `src/Tests/CashPrism.Infrastructure.Tests.Unit` | `Infrastructure` without a database: the password hasher |
| `src/Tests/CashPrism.Shell.Tests.Integration` | `Shell` end to end, hosting included, and the import through the objects the host actually wires |
| `src/Tests/CashPrism.Web.Tests.Unit` | `Web` without a host: pages, layout and components rendered with bUnit against test doubles, and the formatting and mapping beside them |
| `src/Tests/CashPrism.Anonymiser.Tests.Unit` | Command-line parsing, no file on disk |
| `src/Tests/CashPrism.Anonymiser.Tests.Integration` | The file round trip, built in code — no binary fixture in the repository |
| `src/Tests/CashPrism.DemoData.Tests.Unit` | The generator in memory: command line, identifiers, and one test per special case the data covers |
| `src/Tests/CashPrism.DemoData.Tests.Integration` | The written workbook: repeatable cell content, a file the export reader reads, and the committed demo export still matching the generator |

`src/Tests/CashPrism.TestSupport` sits in the same folder and is not a test
project: it holds fixtures that more than one test project needs, contains no
tests of its own, and carries neither the test SDK nor a runner, so `dotnet
test` passes over it. Today that is `XlsxTestWorkbook`, which builds a
Finanzguru-shaped `.xlsx` in memory, `FinanzguruTestRow`, which fills one data
row of it, and the stand-ins the import use case runs on in a test:
`FakeImportSource`, `FakeImportStore` and `FixedClock`; and `TestBookings`,
which builds a booking with plain values for every field a test does not name;
and `DemoSample`, which embeds the committed demo export
`samples/demo-export.xlsx`.
No production project may reference it, and `CashPrism.Architecture.Tests`
fails the build if one does. When a fixture belongs there is a rule rather than a description, and it
is in [`../AGENTS.md`](../AGENTS.md#test-projects).

## Directory layout

```
design/
samples/
src/
  CashPrism.slnx
  CashPrism.Domain/
  CashPrism.Application/
  CashPrism.Infrastructure/
  CashPrism.Infrastructure.Finanzguru/
  CashPrism.Web/
  CashPrism.Shell/
  Tests/
    CashPrism.Architecture.Tests/
    CashPrism.Application.Tests.Unit/
    CashPrism.Domain.Tests.Unit/
    CashPrism.Infrastructure.Finanzguru.Tests.Unit/
    CashPrism.Infrastructure.Finanzguru.Tests.Integration/
    CashPrism.Infrastructure.Tests.Integration/
    CashPrism.Shell.Tests.Integration/
    CashPrism.Web.Tests.Unit/
    CashPrism.Anonymiser.Tests.Unit/
    CashPrism.Anonymiser.Tests.Integration/
    CashPrism.DemoData.Tests.Unit/
    CashPrism.DemoData.Tests.Integration/
    CashPrism.TestSupport/
  Tools/
    CashPrism.Anonymiser/
    CashPrism.DemoData/
```

The solution file is `src/CashPrism.slnx`. `src/Tests` and `src/Tools` are
plain folders inside the solution root, and the `/Tests/` and `/Tools/`
solution folders in the `.slnx` list the same projects.

`samples/` holds the demo export a person without data of their own tries
CashPrism on; see [`demo-data.md`](demo-data.md).

`design/` holds the design system as exported from Claude Design: tokens,
components, guidelines and brand assets. It is reference material for the UI
and is not part of the build.

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
  ClosedXML stays contained. Adding Finanzguru's successor does not touch the
  Finanzguru parser.
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
  so a real Finanzguru export stays recognisable as one.
- **`DemoData` is a development tool as well.** It is the third composition
  root, writes its workbook with the ClosedXML the export reader already
  brings, and takes the column names from `Infrastructure.Finanzguru` so the
  file it writes and the reader that reads it cannot drift apart. See
  [`demo-data.md`](demo-data.md).
- **The migrations live with the schema they describe.** They sit in
  `CashPrism.Infrastructure/Persistence/Migrations`, next to the context and the
  entity configurations, and an `IDesignTimeDbContextFactory` lets
  `dotnet ef migrations add` read the model from that project alone. The
  alternative — pointing the tools at `Shell` — would make the composition root a
  build-time dependency of the schema. `dotnet-ef` is pinned in
  `.config/dotnet-tools.json`, so `dotnet tool restore` gives everyone the same
  version instead of whatever is installed globally.
- **The host migrates through an interface.** `Shell` calls
  `IDatabaseMigrator`, declared in `Application`, before the server starts
  listening. It therefore knows that the database has to be brought up to date,
  but not what the database is.
- **`CashPrism.Architecture.Tests` enforces the reference graph.** The rules
  above are not a gentleman's agreement: the build fails if a project outside
  `Shell`, `Anonymiser` and `DemoData` takes a dependency on an Infrastructure
  project.
