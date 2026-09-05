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

One machine runs CashPrism. It is a single executable — no setup, no runtime to
install, no Docker.

Every other device on the home network opens the URL in a browser: laptop,
tablet, phone.

Access is protected by one shared password, so not everyone in the household
sees the finances. The database sits as a file next to the application.

## Status

Early development. Nothing runnable yet.

## License

MIT
