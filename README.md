# CashPrism

Your Finanzguru data on a big screen.

## Why

Finanzguru runs on your phone. Small screen, little visible at once, and you get
the analyses Finanzguru decided to give you.

CashPrism reads Finanzguru's data exports and turns them into something you can
analyse yourself — on a real monitor, with your own filters and charts.

Your data stays with you. No cloud, no account, no signing up anywhere.

## What it does

- Reads Finanzguru exports (`.xlsx`)
- Accumulates them over time instead of replacing them
- Shows transactions, categories and trends you can search and filter
- Runs on Windows, Linux and macOS without an installer

## Imports accumulate

Every import adds to your history instead of replacing it, so you can export and
import every month and keep years of bookings even though a single export only
covers a time window.

A booking is matched across exports by the identifier Finanzguru gives it, so
re-importing the same export changes nothing. If a booking was edited in
Finanzguru between two exports — a corrected counterparty, a re-assigned
category — CashPrism takes the newer version rather than keeping both. Only rows
that are new or changed are stored; the export file itself is not kept.

## Running it

One machine runs CashPrism. It will be a download per platform with a single
executable inside — no setup, no runtime to install. On a home server or a NAS
it also runs as a container, see
[docs/hosting.md](docs/hosting.md#in-a-container). Until the first release it is
started from the source tree, see [Getting started](#getting-started).

Every other device on the home network opens the URL in a browser: laptop,
tablet, phone.

Access is protected by one shared password, so not everyone in the household
sees the finances. The database sits as a file next to the application.

## Getting started

There is no packaged download yet, so CashPrism is built from source. You need
the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/raisr/CashPrism.git
cd CashPrism
dotnet run --project src/CashPrism.Shell
```

CashPrism opens your browser and prints every address it can be reached at:

```
CashPrism 0.1.0

On this machine:
  http://localhost:5080

From another device in the same network:
  http://192.168.1.7:5080

Press Ctrl+C to stop.
```

More than one line under *From another device* is normal — pick the one the
other device answers on.

No Finanzguru export at hand? Import
[`samples/demo-export.xlsx`](samples/demo-export.xlsx): three years of a
fictional household, made up from end to end, in the shape a real export has.
[docs/demo-data.md](docs/demo-data.md) says what it contains.

A different port, no browser window, a build that runs without the SDK, a
container for a home server or a NAS, and the settings that make it stick: see
[docs/hosting.md](docs/hosting.md).

## Using it

[docs/benutzung.md](docs/benutzung.md) explains how to use CashPrism: starting
it, reaching it from another device, importing an export and reading what was
stored.

That guide is German, and so is the interface. Finanzguru, the only source
CashPrism reads, is sold in German-speaking markets only, so every person using
CashPrism reads German. Should CashPrism learn to read a source whose users do
not, English documentation becomes worth adding.

## Documentation

[docs/README.md](docs/README.md) lists what is written down and where.

## Status

Early development. You can upload a Finanzguru export, page and sort through
the bookings it stored, and see what was imported when. The overview shows
income and spending per month and where the money went; the analysis compares
spending by category over three, six or twelve months. There is no filtering
and no searching yet. See [ROADMAP.md](ROADMAP.md) for the planned phases and
[CHANGELOG.md](CHANGELOG.md) for what has landed.

Have an idea or a suggestion? Post it in
[Discussions](https://github.com/raisr/CashPrism/discussions/categories/ideas),
category *Ideas*.

## License

MIT

## Trademark notice

Finanzguru is a registered trademark of dwins GmbH. CashPrism is an
independent project with no connection to dwins GmbH; it reads Finanzguru's
data exports because that is what the application is for.

## Third-party notices

CashPrism is built with open-source packages and ships open-source fonts and
icons, all of them other people's work.
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) names each of them with its
licence and copyright notice and reproduces every licence text in full, as
those licences require. It ships next to the executable produced by
`dotnet publish`.
