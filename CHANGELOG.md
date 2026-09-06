# Changelog

All notable changes to CashPrism are recorded here. The format follows
[Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/).

Versioning is [Semantic Versioning](https://semver.org/spec/v2.0.0.html). The
project is pre-1.0: while the major version is `0`, anything may change between
releases and the minor version is bumped for every notable change.

## [Unreleased]

### Added

- Solution structure under `src/` following the onion architecture from
  `AGENTS.md`: Domain, Application, Infrastructure, Infrastructure.Finanzguru,
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

[Unreleased]: https://github.com/raisr/CashPrism/commits/main
