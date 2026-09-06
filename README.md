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

## Imports are additive

Every import adds, it never overwrites.

Each row gets a fingerprint — a hash over date, amount, currency, account,
counterparty and payment reference. If CashPrism already knows the fingerprint,
the row is skipped. The raw contents of every imported file are stored unchanged
as well.

The result: you can export and import every month. Your history grows, even
though a single export only covers a time window.

The price for this: if you correct something in FinanzGuru, the correction
arrives as an additional row rather than as a change to the existing one.

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

More than one line under *From another device* is normal. VPN, Docker and
Hyper-V adapters each add an address, and CashPrism does not guess which one is
yours — type them into the other device until one answers.

### The other ways to start it

| You want | Do this |
|---|---|
| A different port | `dotnet run --project src/CashPrism.Shell -- --port 5099` |
| No browser window | `dotnet run --project src/CashPrism.Shell -- --no-browser` |
| A build you can run without the SDK on the machine | `dotnet publish src/CashPrism.Shell -c Release -o out`, then start `out/CashPrism.Shell` |
| The settings to stick | The `Hosting` section of `src/CashPrism.Shell/appsettings.json` |

The switches win over `appsettings.json` and can be used together:
`-- --port 5099 --no-browser`. Note the bare `--`: it separates the arguments for
`dotnet run` from the arguments for CashPrism.

A single self-contained executable per platform — the double-click case, without
any .NET installed — is still on the roadmap.

### Settings

| Key | Default | Meaning |
|---|---|---|
| `Hosting:Port` | `5080` | The port to listen on. CashPrism binds every network interface, otherwise no other device could reach it |
| `Hosting:DataDirectory` | `data` | Where the database and the stored imports live. A relative path sits next to the executable |
| `Hosting:LaunchBrowser` | `true` | Whether the local browser opens on start |

If the port is already taken, CashPrism says so and stops — it does not quietly
move to another one, because then nobody would know which address to type.

Connections are plain HTTP. A self-signed certificate would mean a security
warning on every phone and tablet in the house, so CashPrism does not pretend to
offer encryption it cannot deliver on a home network.

## Status

Early development. It starts and serves a page, but there is no import and no
password yet. See [ROADMAP.md](ROADMAP.md) for the planned phases and
[CHANGELOG.md](CHANGELOG.md) for what has landed.

## License

MIT
