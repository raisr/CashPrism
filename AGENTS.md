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

CashPrism reads data exports from the personal finance app "FinanzGuru" and
makes your own finances analysable on a large screen.

Imports are additive for the raw data and projective for the result. A booking
is identified by its FinanzGuru `Buchungs-ID`, never by a hash over its fields:
the same booking is enriched between exports, so a re-import overwrites the
stored `Booking` with the later state instead of adding a second row.
"Later" is the export date parsed from the sheet name
(`YYYYMMDD_Export_Alle_Buchungen`), falling back to the more recent import run
when that name cannot be parsed. Of the raw rows, only those new or changed
since the last import are kept, and the `.xlsx` file itself is not stored. The
measurements these rules rest on are in
[`docs/finanzguru-export.md`](docs/finanzguru-export.md).

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

**Money is stored as whole cents in an integer column, never as `decimal`.**
SQLite has no decimal type, so EF Core keeps a `decimal` as text, and text
compares lexicographically: `ORDER BY` and `SUM` over an amount then return
nonsense. The models keep `decimal`; the conversion belongs to the persistence
configuration. An amount finer than a cent is refused rather than rounded.

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

`Shell` and `Anonymiser` are the only two projects allowed to reference an
Infrastructure project — both are composition roots.
`CashPrism.Architecture.Tests` fails the build if a project outside that set
takes such a dependency.

Consequences worth stating, because they are where it usually goes wrong:

- `Web` is a library, not a host. It never references an Infrastructure
  project, never reads configuration and has no `Program.cs`. Everything that
  only makes sense once the process is running belongs in `Shell`.
- `Shell` contains no business logic and no UI. If something there gets
  interesting enough to test, it is in the wrong project.
- `Anonymiser` is a development tool, not part of the shipped application. It
  never references ClosedXML — an `.xlsx` is a zip it takes apart and puts back
  together itself, so a real FinanzGuru export stays recognisable as one. This
  is about the fidelity of the output, not about containing a dependency.

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

`Web` must stay hostable without `Shell` — that is what the integration tests
use.

## Language of the user interface

The UI is **German**. FinanzGuru, the only source CashPrism reads, is sold in
German-speaking markets only, so every person this application has is a German
reader. Internationalisation is nonetheless in place from the first screen — it
is cheap now and expensive to retrofit.

- **No translatable text as a literal in markup or code.** Every user-visible
  string comes from `IStringLocalizer<Strings>`, backed by
  `src/CashPrism.Web/Resources/Strings.resx`. Product names — "CashPrism" — are
  not translatable and stay literals.
- **Keys are English, values are German.** Everything else in the repository
  stays English: identifiers, comments, XML docs, tests, commits. The deviation
  from `core.language` is limited to resource values and recorded below.
- **The resource file is neutral.** `Strings.resx` is what ships; there is no
  English sibling to keep in step. A second language is a new
  `Strings.<culture>.resx` and nothing else.
- **The culture is set by the host, never inherited.** `Shell` pins `de-DE`, so
  an English Windows cannot format amounts and dates one way while the labels
  beside them read another. `Web` never reads it from configuration.

## Domain terms

| Term | Means |
|---|---|
| Import run | One processed export file, recorded with the file hash |
| Raw row | A row from an imported file, stored verbatim as JSON — kept only when it is new or has changed since the last import |
| Fingerprint | The FinanzGuru `Buchungs-ID` (column Z): 40 hex characters, unique per booking and stable across exports. It identifies a booking. The field-hash it replaced collapsed 46 groups of distinct bookings and dropped 52 of them in a single 6,324-row export |
| Booking | A single booking, keyed by its fingerprint. A projection of the latest export that carries the booking — overwritten on re-import, not an immutable record |

## Deviations from the shared rules

Rules from `AGENTS.core.md` or a stack pack that deliberately do not apply here.
Every row was decided once and is recorded in `devkit.lock.json`, so
`/devkit-sync` does not ask again until the upstream rule changes.

| Rule | Deviation | Why |
|---|---|---|
| `core.language` | User-visible UI text is German: the values in `Strings.resx` are German, and so is what the application renders | FinanzGuru is sold in German-speaking markets only, so the UI's only audience reads German. Everything else stays English — keys, identifiers, comments, XML docs, tests, commits, tickets and this file |
