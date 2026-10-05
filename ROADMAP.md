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

A standalone console tool that turns a real Finanzguru `.xlsx` into one that is
safe to share.

- Reads a Finanzguru export and writes an anonymised copy: counterparty,
  payment reference, account name and IBAN replaced; amounts optionally scaled;
  file structure kept 1:1.
- The Finanzguru column mapping lands in `CashPrism.Infrastructure.Finanzguru`
  and is reused by the real parser in M3.

## M3 — Import end to end

Upload a Finanzguru export, store it deduplicated, and see the result as data.

- EF Core + SQLite at `./data/cashprism.db`; migrations applied on startup
  before the first request is served.
- Single-instance guard: refuse to start a second process against the same
  database file.
- `Domain` models: Booking, ImportRun, RawRow, with the identity rule — a
  booking is keyed by its Finanzguru `Buchungs-ID`.
- ClosedXML parser for the Finanzguru `.xlsx`, isolated in
  `CashPrism.Infrastructure.Finanzguru`.
- Import use case in `Application`: a `Booking` is a projection overwritten
  by the later export; raw rows are kept only when new or changed; the `.xlsx`
  itself is not stored.
- Upload UI and a list of past import runs.

## M4 — Design system

Give the existing application the look of the design system under
[`design/`](design/), before more screens are built in the old one.

- Fonts, icons and logo bundled with the application; nothing is fetched from
  the internet.
- Theme and design tokens for both light and dark.
- The shell: sidebar, top bar and page header.
- The booking list and the import page restyled, with the data stored today.

## M5 — First release

The first version someone else can download: analyses on the data imported
today, a demo export to try them on, and the application secured and packaged.

- The remaining useful export columns stored on the booking (#104).
- A settings page that deletes all data, so a fresh import rebuilds everything.
- The anonymiser moved to `src/Tools/`, in a `Tools` solution folder.
- `CashPrism.DemoData`: a generator for a larger synthetic Finanzguru export.
  The generated file is committed and linked from the README.
- Analyses: average spending per category over the last 12 months, and income
  against spending per month — each with a tile on the overview.
- What grew markedly between two periods.
- Auth:
  - One shared password, PBKDF2, cookie authentication.
  - Password supplied out of band (environment variable / user secret), never
    in the repository.
  - `[Authorize]` is the default; `[AllowAnonymous]` is the justified
    exception.
- Delivery:
  - Self-contained single-file binary per platform (Windows, Linux, macOS).
  - A repeatable release process, including cutting a `CHANGELOG.md` version
    at the tag.
  - The demo export attached to every release.

## M6 — Accounts and contracts

- Booking list with filters (account, category, period), search, and the
  remaining detail fields.
- Accounts in user-defined groups, with a balance per account.
- Contracts grouped by interval.
- Forecasts of contract bookings, with an overdue marker.

## M7 — Net worth

- Net worth over time.
- Tangible assets entered by hand, such as real estate.

## M8 — Reports and custom analyses

- Monthly report, annual review, print/PDF.
- Custom analyses.
