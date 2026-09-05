# Roadmap

Where CashPrism is going, in coarse phases. No dates, no estimates. Each phase
is broken into GitHub issues when it is picked up — this file stays the big
picture, the issues are the work.

Phases are ordered by dependency: a later phase assumes the earlier ones are in
place.

## M1 — It runs

Prove the whole wiring end to end.

- Blazor start page rendered with `InteractiveServer`, showing the version and a
  visible "wiring ok" marker.
- `Shell` hosting basics: bind Kestrel to `0.0.0.0` on a fixed, overridable
  port; print the reachable LAN URL to the console; open the local browser on
  start unless told not to; create `./data` if missing.

## M2 — Anonymiser

A standalone console tool that turns a real FinanzGuru `.xlsx` into one that is
safe to share.

- Reads a FinanzGuru export and writes an anonymised copy: counterparty,
  payment reference, account name and IBAN replaced; amounts optionally scaled;
  file structure kept 1:1.
- The FinanzGuru column mapping lands in `CashPrism.Infrastructure.Finanzguru`
  and is reused by the real parser in M5.

## M3 — Persistence

- EF Core + SQLite at `./data/cashprism.db`; migrations applied on startup
  before the first request is served.
- `Domain` models: Transaction, ImportRun, RawRow, and the fingerprint rule.
- Single-instance guard: refuse to start a second process against the same
  database file.

## M4 — Auth

- One shared password, PBKDF2, cookie authentication.
- Password supplied out of band (environment variable / user secret), never in
  the repository.
- `[Authorize]` is the default; `[AllowAnonymous]` is the justified exception.

## M5 — Import

- ClosedXML parser for the FinanzGuru `.xlsx`, isolated in
  `CashPrism.Infrastructure.Finanzguru`.
- Import use case in `Application`: additive, deduplicated by fingerprint, raw
  rows and the file hash stored unchanged.
- Upload UI and a list of past import runs.

## M6 — Analysis UI

- Transaction list with search and filter.
- Categories.
- Trends and charts.

## M7 — Delivery

- Self-contained single-file binary per platform (Windows, Linux, macOS).
- A repeatable release process, including cutting a `CHANGELOG.md` version at
  the tag.
