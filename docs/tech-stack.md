# Tech stack

What CashPrism is built on, and why each piece was picked. This document
describes the current state; the rules that constrain the stack — the database
on a local disk only, no DDD — are binding and live in
[`Agents.md`](../Agents.md#tech-stack).

| Area | Choice | Why |
|---|---|---|
| Runtime | .NET 10 (LTS) | Long-term support, and the only runtime the project targets. Set once in `src/Directory.Build.props` |
| UI | Blazor Web App, render mode `InteractiveServer` | One language for server and browser. The state lives on the hosting machine, so the phone in the kitchen renders the same session without a separate API |
| Web server | Kestrel, built in | Already part of ASP.NET Core. A reverse proxy would be one more thing to install on a home machine for no gain |
| Database | SQLite via EF Core, `./data/cashprism.db` | A single file next to the executable, no server to run. EF Core because the migrations run on startup |
| Excel | ClosedXML | Reads `.xlsx` without Excel installed. It stays inside `CashPrism.Infrastructure.Finanzguru` so nothing else depends on it |
| Auth | One shared password, PBKDF2, cookie authentication | The household shares one login; there are no user accounts to manage. PBKDF2 comes from the BCL, so no extra dependency |
| Delivery | Self-contained single-file binary per platform | The person running CashPrism double-clicks it. No installer, no runtime to install, no Docker |

## What already exists

The table above is the decided stack, not a report of finished work. In the
source tree today:

- .NET 10, Kestrel and Blazor are in place, and `CashPrism.Shell` hosts the app
- ClosedXML and `Microsoft.EntityFrameworkCore.Sqlite` are referenced
- Authentication is not built yet
- Delivery is still "run it from the source tree" — see
  [hosting.md](hosting.md) for the ways to start it

Package versions are not repeated here; they are in the `.csproj` files, which
cannot go stale.
