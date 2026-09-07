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
- `ROADMAP.md` with the planned phases M1 through M7.
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
  It shows the application version and a marker that switches from `Prerendered`
  to `Interactive` as soon as the browser's connection to the server is live —
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

[Unreleased]: https://github.com/raisr/CashPrism/commits/main
