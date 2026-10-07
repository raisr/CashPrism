# AGENTS.md — CashPrism

Binding rules for anyone working in this repository, human or agent.
**It prescribes, it does not describe** — explanatory prose belongs in `docs/`,
and this file links to it.

## Shared rules

These files come from [raisr/devkit](https://github.com/raisr/devkit) and are
updated by `/devkit-sync`. Do not edit them here; change them upstream.

The `@` lines below are imports, not links: they pull the file into the
session. Turning one into a Markdown link silently switches the rules off.

@AGENTS.core.md — rules that hold in every repository
@AGENTS.dotnet.md — rules for dotnet
@AGENTS.dotnet-core.md — rules for dotnet-core
@AGENTS.github.md — conventions for github

@AGENTS.local.md — personal, machine-specific, git-ignored; wins on conflicts.
It is imported the same way, and the import is simply ignored where the file
does not exist.

## Overview

CashPrism reads data exports from the personal finance app "Finanzguru" and
makes your own finances analysable on a large screen.

Imports are additive for the raw data and projective for the result. A booking
is identified by its Finanzguru `Buchungs-ID`, never by a hash over its fields:
the same booking is enriched between exports, so a re-import overwrites the
stored `Booking` with the later state instead of adding a second row.
"Later" is the export date parsed from the sheet name
(`YYYYMMDD_Export_Alle_Buchungen`), falling back to the more recent import run
when that name cannot be parsed. Of the raw rows, only those new or changed
since the last import are kept, and the `.xlsx` file itself is not stored. The
measurements these rules rest on are in
[`docs/finanzguru-export.md`](docs/finanzguru-export.md).

**CashPrism mirrors the export; it never edits what the export carries.**
A category, a transfer flag, a contract or anything else a Finanzguru row says
is changed in Finanzguru and arrives with the next export. A change made here
would be overwritten by the next import or contradict it. CashPrism keeps
state of its own only for what the export carries nothing about.

## Trademark

Finanzguru is a registered trademark of dwins GmbH, unrelated to this project.
The spelling is `Finanzguru` — capital F, lower-case g — everywhere in prose;
the identifiers already have it right. No Finanzguru logo or other figurative
mark is ever used, here or in any built artifact.

One machine hosts the application, every other device on the home network
reaches it through a browser. Everything stays local: no cloud, no external
account.

See `README.md` for the user-facing description.

## Tech stack

For what CashPrism is built on — runtime, UI, web server, database, Excel
library, auth and delivery format — and why each piece was picked, see
[`docs/tech-stack.md`](docs/tech-stack.md). The rule below binds whatever that
document ends up listing.

The database lives on a local disk only, never on a network share. SQLite
locking over SMB is unreliable. This is a decision, not an oversight — do not
propose moving it.

**Money is whole cents in a `long`, everywhere — model, database, use cases.**
A fractional type buys nothing here and costs twice: SQLite has no decimal type,
so EF Core keeps a `decimal` as text, and text compares lexicographically, which
makes `ORDER BY` and `SUM` over an amount return nonsense. The unit belongs in the
name (`AmountInCents`), because a bare `Amount` leaves every reader guessing.
Formatting for a person is the only place the number is divided, and that belongs
to the UI. The scale assumes a currency with two decimal places, which is what a
Finanzguru export carries.

## Architecture

For the projects that exist today, the directory layout and why the solution is
cut this way, see [`docs/architecture.md`](docs/architecture.md). The rules
below bind regardless of how many projects that document ends up listing.

**The infrastructure layer is split into more than one project here.**
`CashPrism.Infrastructure` holds persistence, `CashPrism.Infrastructure.Finanzguru`
holds the export reader. `core.architecture` leaves that split to the
repository, and this is where it is written down: a second external source
becomes a new `CashPrism.Infrastructure.<Name>` project next to the existing
one, not a change to the existing parser.

`Shell`, `Anonymiser` and `DemoData` are the only **production** projects
allowed to reference an Infrastructure project — all three are composition
roots.
`CashPrism.Architecture.Tests` fails the build if a production project outside
that set takes such a dependency.

**The rule does not bind test projects.** A test project may reference whatever
it has to in order to assert something, an Infrastructure project included: it
wires nothing for a use case to run on, it checks what the wiring does. That is
how a test covering the seam between two Infrastructure projects is written at
all, and `ProjectAssemblies` therefore lists only the production assemblies.

Consequences worth stating, because they are where it usually goes wrong:

- `Web` is a library, not a host. It never references an Infrastructure
  project, never reads configuration and has no `Program.cs`. Everything that
  only makes sense once the process is running belongs in `Shell`.
- `Shell` contains no business logic and no UI. If something there gets
  interesting enough to test, it is in the wrong project.
- The component library is a detail of `Web`. No other project references it —
  not `Shell`, which starts the process without knowing how a page is drawn,
  and nothing inside the onion. `CashPrism.Architecture.Tests` fails the build
  otherwise.
- `Anonymiser` is a development tool, not part of the shipped application. It
  never references ClosedXML — an `.xlsx` is a zip it takes apart and puts back
  together itself, so a real Finanzguru export stays recognisable as one. This
  is about the fidelity of the output, not about containing a dependency.
- `DemoData` is a development tool, not part of the shipped application. What it
  writes is synthetic from end to end: no value from a real export enters the
  generator, not even as a seed or a template.
- **A development tool lives under `src/Tools/`** and in the `/Tools/` solution
  folder, apart from the application it serves. Its test projects stay under
  `src/Tests/` like every other.

## Test projects

`dotnet.tests` names one project that carries no `.Unit`/`.Integration` suffix:
`CashPrism.Architecture.Tests`, which asserts solution-wide rules. This
repository has a second, and it is the only one:

**`src/Tests/CashPrism.TestSupport` holds fixtures shared between test
projects.** It contains no tests, is not a test project — no test SDK, no
runner, so `dotnet test` skips it rather than failing on an assembly without
tests — and no production project may reference it.
`CashPrism.Architecture.Tests` fails the build otherwise.

- **A fixture belongs there once at least two test projects use it, and not
  before.** One user is not a shared fixture; it is a fixture that lives next to
  its test. Moving it early costs every reader a project hop for nothing.
- **Nothing else moves in with it.** A helper only one project needs stays in
  that project, however tempting the shared home looks.

## Hosting

CashPrism is shipped as one executable that a person double-clicks, so `Shell`
carries the concerns a web project normally does not have:

- Bind Kestrel to `0.0.0.0` on a fixed, overridable port. Binding to localhost
  only would lock out every other device.
- Create `./data` if missing and apply EF Core migrations before serving the
  first request.
- Print the reachable LAN URL to the console so it can be typed on a phone.
- Open the local browser on start, unless started with a switch that says
  otherwise.
- Refuse to start a second instance against the same database file.

**The shape of the published build is set in `CashPrism.Shell.csproj`, never on
a publish command line.** `SelfContained`, `PublishSingleFile` and the like
change which packages are resolved, and `THIRD-PARTY-NOTICES.md` is generated
from exactly that set — a flag passed on the command line would leave the
notices describing a build nobody distributes.

- **The one input a publish takes is `-r <rid>`**, with a runtime identifier
  from the project's `RuntimeIdentifiers`: `win-x64`, `linux-x64`, `osx-x64`,
  `osx-arm64`. That publish is a self-contained single file. Any other
  identifier fails the build; a new platform is a change to that list.
- **Without `-r`, the publish is portable and framework-dependent** — the build
  the container image runs.
- The notices generator publishes for every identifier in the list and once
  without one, and fails when they ship different packages. One notices file
  covers every build, the .NET runtime of the self-contained ones included.
- **The version comes from the tag only.** `src/Directory.Build.props` carries
  the placeholder `0.0.0-dev`; a release passes `-p:Version=<version>`. The
  version does not change what is resolved, so it is not a shape.

`Web` must stay hostable without `Shell` — that is what the integration tests
use.

**In a container, the image sets what a double-click would otherwise decide.**
The `Dockerfile` is the only place that differs from a desktop start; `Shell`
keeps one code path for both.

- The data directory is `/data`, a volume, writable by the image's non-root
  user. The image never runs as root.
- The browser is never opened, and the start banner names the port instead of
  addresses: inside a container those belong to the container network, which no
  other device reaches. `Shell` tells the two apart by
  `DOTNET_RUNNING_IN_CONTAINER`, which the official .NET images set.
- The port stays 5080, and the single-instance guard and every other hosting
  rule hold unchanged — a second container on the same volume is refused like a
  second process.
- **The image is built from a portable publish**, `dotnet publish -c Release`,
  never with a runtime identifier on the command line, for the reason above. It
  starts through `dotnet CashPrism.Shell.dll`, because the apphost a portable
  publish carries is native to the build machine.
- The image is built and started on every architecture it is published for.
  Building alone does not show that it starts.

## Branches and CI

**`main` is the only long-lived branch.** Every change reaches it through a
pull request from a `feature/` or `fix/` branch (`forge.branches`); `main` is
protected so that no other way in exists, for the maintainer included.

**The gates run in CI on every pull request against `main`.**
`.github/workflows/gates.yml` runs `bash .devkit/gates.sh` on Ubuntu and on
Windows, and a merge needs both green. This adds to `core.gates`, it does not
replace it: the gates still pass locally before a commit.

The job names are the required status checks of the branch protection:
`gates (ubuntu-latest)`, `gates (windows-latest)`, `image (linux/amd64)` and
`image (linux/arm64)`. Renaming a job or changing a matrix means updating the
protection in the same change.

**A release comes only from a version tag on `main`**, created by hand and
never on merge, because it publishes. `.github/workflows/release.yml` refuses a
tag that is not on `main`, runs the gates again, and builds every platform and
the container image from that commit. `vX.Y.Z` needs its `CHANGELOG.md` section,
cut in a release pull request beforehand; `vX.Y.Z-rc.N` needs none and only
creates a draft. The one exception to `main` is a fix to an old release: it is
made on a `release/X.Y` branch, created only then, and tagged there. The steps
are in [`docs/releasing.md`](docs/releasing.md).

## Language of the user interface

The UI is **German**. Finanzguru, the only source CashPrism reads, is sold in
German-speaking markets only, so every person this application has is a German
reader. Internationalisation is nonetheless in place from the first screen — it
is cheap now and expensive to retrofit.

- **No translatable text as a literal in markup or code.** Every user-visible
  string comes from `IStringLocalizer<Strings>`, backed by
  `src/CashPrism.Web/Resources/Strings.resx`. Product names — "CashPrism" — are
  not translatable and stay literals.
- **Keys are English, values are German.** Everything else in the repository
  stays English: identifiers, comments, XML docs, tests, commits. The deviation
  from `core.language` covers the resource values and the user guide, and is
  recorded below.
- **The resource file is neutral.** `Strings.resx` is what ships; there is no
  English sibling to keep in step. A second language is a new
  `Strings.<culture>.resx` and nothing else.
- **The component library reads the same resource file.** MudBlazor ships its
  own English text; a `MudLocalizer` feeds it from `Strings.resx` under
  MudBlazor's own keys, which are written with an underscore
  (`MudDataGrid_Filter`). A key the file does not carry **must** report itself
  as not found, so MudBlazor falls back to its English default — a localiser
  that answers for every key renders raw identifiers in the UI. Leaving a key
  out is therefore a decision, not a defect; answering wrongly is a defect.
- **The culture is set by the host, never inherited.** `Shell` pins `de-DE`, so
  an English Windows cannot format amounts and dates one way while the labels
  beside them read another. `Web` never reads it from configuration.

## Domain terms

| Term | Means |
|---|---|
| Import run | One processed export file, recorded with the file hash |
| Raw row | A row from an imported file, stored verbatim as JSON — kept only when it is new or has changed since the last import |
| Fingerprint | The Finanzguru `Buchungs-ID` (column Z): 40 hex characters, unique per booking and stable across exports. It identifies a booking. The field-hash it replaced collapsed 46 groups of distinct bookings and dropped 52 of them in a single 6,324-row export |
| Booking | A single booking, keyed by its fingerprint. A projection of the latest export that carries the booking — overwritten on re-import, not an immutable record |

## Deviations from the shared rules

Rules from `AGENTS.core.md` or a stack pack that deliberately do not apply here.
Every row was decided once and is recorded in `devkit.lock.json`, so
`/devkit-sync` does not ask again until the upstream rule changes.

| Rule | Deviation | Why |
|---|---|---|
| `core.language` | What a user reads is German: the values in `Strings.resx`, what the application renders, and the user guide `docs/benutzung.md`. It ends once CashPrism reads a second source whose users do not read German — then English user documentation is due | Finanzguru is sold in German-speaking markets only, so the application's only audience reads German. Everything else stays English — keys, identifiers, comments, XML docs, tests, commits, tickets, the rest of `docs/` and this file |
| `dotnet.tests` | A second project under `src/Tests/` carries no `.Unit`/`.Integration` suffix and belongs to no production project: `CashPrism.TestSupport`, described under [Test projects](#test-projects) | `XlsxTestWorkbook` builds a Finanzguru-shaped workbook in code. The anonymiser's round-trip tests need it and the export reader's tests need the same thing, so the alternatives are a second copy or a binary fixture in the repository. The rule itself asks for a ticket arguing the case; that is issue #26 |
| `core.tests` | A maintenance script under `.devkit/` carries no unit tests. It ships in nothing, and a gate that runs it end to end on every commit covers it instead. Logic that is not a maintenance script is not covered by this row, wherever it lives | `.devkit/generate-third-party-notices.cs` is checked by the `notices` gate, which runs the whole generator and diffs its output against the committed `THIRD-PARTY-NOTICES.md` — a golden-file test of the real pipeline, including the publish and the licence metadata lookup that unit tests would have to fake. A second project to make three string functions unit-testable would buy less coverage than the gate already gives |
