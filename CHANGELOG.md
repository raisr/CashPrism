# Changelog

All notable changes to CashPrism are recorded here. The format follows
[Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/).

Versioning is [Semantic Versioning](https://semver.org/spec/v2.0.0.html). The
project is pre-1.0: while the major version is `0`, anything may change between
releases and the minor version is bumped for every notable change.

## [Unreleased]

### Added

- Solution structure under `src/` following the onion architecture from
  `Agents.md`: Domain, Application, Infrastructure, Infrastructure.Finanzguru,
  Web, Shell, and the Unit and Integration test projects.
- `Web` exposes `AddCashPrismWeb()` / `MapCashPrismWeb()` so it stays hostable
  without `Shell`; `Shell` is the composition root.
- Architecture test that fails if the layer dependency rules are violated, and
  a host-boot integration test.
- `ROADMAP.md` with the planned phases M1 through M6.
- This changelog.
- Hosting basics: the application listens on every network interface on port
  `5080`, so any device in the household reaches it, and prints the addresses it
  can be typed as on start — one line per network the machine is on. It creates
  its data directory next to the executable and opens the local browser once the
  server is up. `--port <number>` and `--no-browser` override both, as does the
  `Hosting` section in `appsettings.json`. A port that is already taken ends in
  one readable line instead of a stack trace. Connections are plain HTTP on
  purpose: a self-signed certificate would mean a warning on every phone and
  tablet in the house.
- Start page at `/`, rendered by Blazor with the interactive server render mode.
  It shows the application version and a marker that switches from `Vorgerendert`
  to `Interaktiv` as soon as the browser's connection to the server is live —
  visible proof that the whole chain works.
- The Finanzguru column mapping, in `CashPrism.Infrastructure.Finanzguru`:
  resolves a header row to its known columns by name, never by position, and
  fails loudly on a column that is missing, duplicated or unrecognised.
- `CashPrism.Anonymiser`, a standalone console tool that takes a real
  Finanzguru export apart and puts it back together: `CashPrism.Anonymiser
  <input.xlsx> [<input2.xlsx> …] --out <directory> [--force]`. Every zip entry
  is copied through unchanged; a worksheet using shared strings, or a header
  row with a missing, duplicated or unrecognised column, aborts with a message
  naming the reason instead of guessing.
- The anonymiser now replaces the columns that identify people and accounts —
  `Referenzkonto`, `Name Referenzkonto`, `Beguenstigter/Auftraggeber`,
  `IBAN Beguenstigter/Auftraggeber`, `Verwendungszweck`, `Mandatsreferenz`,
  `Glaeubiger-ID`, `Analyse-Vertrags-ID`, `Buchungs-ID`, `Referenz-Original-ID`
  and `Tags` — with sequential placeholders such as `Counterparty 001` or
  `Account 01`, consistent across every input file of one run: the same
  original value always yields the same replacement, an IBAN stays IBAN-shaped
  and an email address stays email-shaped, and an own account reappearing as a
  counterparty keeps the same placeholder in both roles. Every other column is
  kept byte-identical. A self-check reads the written file back and aborts,
  deleting the incomplete output, if a replaced column still carries an
  original value. See [`docs/anonymiser.md`](docs/anonymiser.md).
- The anonymiser gained `--scale <factor>` and `--max-rows <n>`: `--scale`
  multiplies `Betrag` and `Kontostand` together by a factor, rounded to two
  decimals, to hide the absolute level of a shared fixture; `--max-rows` keeps
  only the newest `n` data rows per file, applied before the replacement so
  placeholder numbers stay dense. Neither is a privacy safeguard — see
  [`docs/anonymiser.md`](docs/anonymiser.md) for what they do not protect
  against.
- The anonymiser gained `--synthetic-values`: it replaces `Buchungstag`,
  `Betrag`, `Kontostand` and the four period columns with generated values, so
  the output carries no date or amount of the real export while still reading
  as one. Meant for fixtures in a public repository; without the switch a
  shared file keeps its real dates and amounts as before.
- The application now keeps a database. On start it creates `./data/cashprism.db`
  if it is missing and brings it up to the schema the build expects, before the
  first page is served — so an update that changes the schema needs no migration
  step from the person running it. There is nothing in the database yet: the
  import that fills it comes next. Amounts are held as whole cents, which is what
  makes sorting and totalling them reliable.
- The application now has an interface to hang pages on: a navigation drawer
  with Übersicht, Buchungen and Import, a bar across the top naming the page you
  are on, and a light and a dark appearance that follow the setting of the
  machine you look at it from until you flip the switch in that bar. On a phone
  the drawer folds away behind the menu button, so the same screens work in the
  kitchen and at the desk. Nothing is fetched from the internet — no web font,
  no CDN — so it looks the same on a machine with no connection at all. The
  overview and the booking list are still empty; the booking list fills next.
  See [`docs/ui.md`](docs/ui.md).

- A Finanzguru export can now be imported. Handing CashPrism a file stores the
  bookings it describes and records what the import did: how many rows were
  read, how many bookings were new, how many were replaced by a newer state and
  how many said nothing that was not already known. Uploading the same file
  twice is not an error — it is recognised by its content and reported as
  already imported, having changed nothing. Because Finanzguru enriches a
  booking after the fact, a later export replaces what is stored, while an
  older one cannot undo a newer one. Only rows that are new or changed are
  kept, so importing daily does not grow the database with copies of what it
  already holds. See [`docs/finanzguru-export.md`](docs/finanzguru-export.md).
- The Import page now takes a file. Pick a Finanzguru `.xlsx` and it is read
  and stored on the spot, with the result underneath: how many rows the file
  carried, how many bookings were new, how many were updated and how many said
  nothing new. A file the export gained a column in is imported anyway and the
  column is named, so a change to Finanzguru's format is visible instead of
  silent. A file that is not a Finanzguru export is refused, and one larger
  than 64 MB is not read at all — roughly fifty times the size of a real
  export. A refused file says why, in German, on the page: which column is
  missing, which row carries a value that is not a date or an amount, or that
  the worksheet name carries no export date. The first 20 reasons are listed
  and the rest counted; all of them go to the console as well.
- `THIRD-PARTY-NOTICES.md` lists the name, version, licence and copyright of
  every package the published build actually ships, and reproduces the full
  text of every licence it names — which is what MIT and Apache-2.0 actually
  ask a distributor to pass on. It is generated from `CashPrism.Shell`'s own
  publish output rather than typed by hand, and ships next to the executable
  that `dotnet publish` produces. Where a package declares no copyright notice
  at all, the file says so instead of guessing one from its author list.
- Only one CashPrism at a time may use a database. Starting a second one
  against the same data directory stops with a line naming the database file
  instead of two processes writing to it — SQLite tolerates that badly enough to
  lose data. Two instances on two data directories are unaffected, and the
  database is free again the moment the owning process ends, a crash included.
  See [`docs/hosting.md`](docs/hosting.md).
- The Buchungen page now shows the bookings. Date, account, counterparty,
  payment reference, category and amount, newest first, with every column
  sortable and pages of 25, 50 or 100 rows. Amounts always carry their sign and
  two decimals, and a credit and a debit are told apart by that sign before
  their colour. While nothing is imported the page says so and offers the way to
  the upload instead of an empty table. Sorting and paging happen in the
  database rather than in the browser, so the measured export of 6,327 bookings
  is one screen of rows over the network and not six thousand.
  The pager counts in German, separators included.
  See [`docs/ui.md`](docs/ui.md).
- A new page, Importverlauf, lists what was imported and when: the time of the
  run, the file it read, the export date taken from the sheet name, the file
  checksum and the four counts the import reported — rows read, bookings new,
  updated and unchanged. Newest first. It is what makes an import something you
  can look back at rather than a number that was on screen once.
  The time an import ran is now stored without its time-zone offset, which is
  what lets the database sort it; existing databases are rewritten on the next
  start, and nothing about the recorded times changes.
  See [`docs/ui.md`](docs/ui.md).
- A user guide in German, [`docs/benutzung.md`](docs/benutzung.md): how to
  start CashPrism and reach it from another device, what each page offers, what
  the four import counts mean, what to do when a file is refused, and what the
  booking list cannot do yet. The README links to it and says why it is German.
- CashPrism has its own browser-tab icon, and ships the fonts (Manrope,
  JetBrains Mono) and the icon set (Lucide) of its design system with the
  application instead of fetching them from the internet — nothing changes on
  screen yet. [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md) now lists
  them with their licences beside the packages.
- On a wide screen the navigation collapses to a narrow rail of icons and back,
  for more room on the page. Hovering an icon names its destination.

### Changed

- The user interface is now German. Finanzguru is only available in
  German-speaking markets, so the one audience CashPrism has reads German — the
  start page says `Rendermodus: Vorgerendert`, the page declares itself as
  German, and the application runs with German number and date formats instead of
  whatever the machine it was started on happens to use. Text comes from a
  resource file rather than from the markup, so a further language later is a new
  resource file and not a rewrite.
- The import rules were corrected against two real Finanzguru exports taken one
  day apart. A booking is now identified by its Finanzguru `Buchungs-ID` instead
  of a hash over its fields — that hash collapsed 46 groups of distinct bookings
  and dropped 52 of them in a single 6,324-row export. Because bookings are
  enriched between exports, a re-imported booking now overwrites the stored
  booking with the later state instead of adding a second row, and raw rows
  are kept only when new or changed since the last import; the `.xlsx` file
  itself is no longer stored. No import code exists yet — this is the
  specification the import milestone is built to. See
  [`docs/finanzguru-export.md`](docs/finanzguru-export.md).
- CashPrism takes on the colours and type of its new design: a calm grey-blue
  ground with white cards, one blue for everything you can act on, deep navy
  in the dark theme, and the bundled Manrope typeface throughout. Dates and
  amounts keep lining up digit under digit, now in Manrope's tabular figures
  instead of a monospace face. See [`docs/ui.md`](docs/ui.md).
- The navigation and the bar above every page follow the new design: the
  destinations that bring data in are grouped under *Daten*, an Import button
  sits in the top bar on every page, and the promise that your data stays on
  this machine is pinned to the bottom of the navigation, together with the
  version. The start page says plainly that nothing has been imported yet and
  leads to the import.

### Fixed

- The interface spells Finanzguru the way its owner does, with a lower-case g.
- On a device set to dark mode, CashPrism now opens dark. It used to open light
  and only followed the system setting once that setting changed.

[Unreleased]: https://github.com/raisr/CashPrism/commits/main
