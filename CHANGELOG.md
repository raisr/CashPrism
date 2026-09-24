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
- The FinanzGuru column mapping, in `CashPrism.Infrastructure.Finanzguru`:
  resolves a header row to its known columns by name, never by position, and
  fails loudly on a column that is missing, duplicated or unrecognised.
- `CashPrism.Anonymiser`, a standalone console tool that takes a real
  FinanzGuru export apart and puts it back together: `CashPrism.Anonymiser
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
- The application now keeps a database. On start it creates `./data/cashprism.db`
  if it is missing and brings it up to the schema the build expects, before the
  first page is served — so an update that changes the schema needs no migration
  step from the person running it. There is nothing in the database yet: the
  import that fills it comes next. Amounts are held as whole cents, which is what
  makes sorting and totalling them reliable.

### Changed

- The user interface is now German. FinanzGuru is only available in
  German-speaking markets, so the one audience CashPrism has reads German — the
  start page says `Rendermodus: Vorgerendert`, the page declares itself as
  German, and the application runs with German number and date formats instead of
  whatever the machine it was started on happens to use. Text comes from a
  resource file rather than from the markup, so a further language later is a new
  resource file and not a rewrite.
- The import rules were corrected against two real FinanzGuru exports taken one
  day apart. A booking is now identified by its FinanzGuru `Buchungs-ID` instead
  of a hash over its fields — that hash collapsed 46 groups of distinct bookings
  and dropped 52 of them in a single 6,324-row export. Because bookings are
  enriched between exports, a re-imported booking now overwrites the stored
  booking with the later state instead of adding a second row, and raw rows
  are kept only when new or changed since the last import; the `.xlsx` file
  itself is no longer stored. No import code exists yet — this is the
  specification the import milestone is built to. See
  [`docs/finanzguru-export.md`](docs/finanzguru-export.md).

[Unreleased]: https://github.com/raisr/CashPrism/commits/main
