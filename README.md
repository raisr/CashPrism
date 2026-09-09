# CashPrism

Your FinanzGuru data on a big screen.

## Why

FinanzGuru runs on your phone. Small screen, little visible at once, and you get
the analyses FinanzGuru decided to give you.

CashPrism reads FinanzGuru's data exports and turns them into something you can
analyse yourself — on a real monitor, with your own filters and charts.

Your data stays with you. No cloud, no account, no signing up anywhere.

## What it does

- Reads FinanzGuru exports (`.xlsx`)
- Accumulates them over time instead of replacing them
- Shows transactions, categories and trends you can search and filter
- Runs on Windows, Linux and macOS without an installer

## Imports accumulate

Every import adds to your history instead of replacing it, so you can export and
import every month and keep years of bookings even though a single export only
covers a time window.

A booking is matched across exports by the identifier FinanzGuru gives it, so
re-importing the same export changes nothing. If a booking was edited in
FinanzGuru between two exports — a corrected counterparty, a re-assigned
category — CashPrism takes the newer version rather than keeping both. Only rows
that are new or changed are stored; the export file itself is not kept.

## Running it

One machine runs CashPrism. It will be a single executable — no setup, no runtime
to install, no Docker. Until then it is started from the source tree, see
[Getting started](#getting-started).

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

A different port, no browser window, a build that runs without the SDK, and the
settings that make it stick: see [docs/hosting.md](docs/hosting.md).

## Documentation

[docs/README.md](docs/README.md) lists what is written down and where.

## Status

Early development. It starts and serves a page, but there is no import and no
password yet. See [ROADMAP.md](ROADMAP.md) for the planned phases and
[CHANGELOG.md](CHANGELOG.md) for what has landed.

## License

MIT
