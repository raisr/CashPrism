# Hosting

How CashPrism is started, what can be configured, and why it behaves the way it
does. For the first steps see the [Getting started](../README.md#getting-started)
section of the README.

## The other ways to start it

| You want | Do this |
|---|---|
| A different port | `dotnet run --project src/CashPrism.Shell -- --port 5099` |
| No browser window | `dotnet run --project src/CashPrism.Shell -- --no-browser` |
| A build you can run without the SDK on the machine | `dotnet publish src/CashPrism.Shell -c Release -o out`, then start `out/CashPrism.Shell` |
| A container on a home server or NAS | See [In a container](#in-a-container) |
| The settings to stick | The `Hosting` section of `src/CashPrism.Shell/appsettings.json` |
| Refresh [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md) after a package, font or icon change | `dotnet run --file .devkit/generate-third-party-notices.cs` (the `notices` gate fails while it is stale) |

The switches win over `appsettings.json` and can be used together:
`-- --port 5099 --no-browser`. Note the bare `--`: it separates the arguments for
`dotnet run` from the arguments for CashPrism.

A single self-contained executable per platform — the double-click case, without
any .NET installed — is still on the roadmap. The shape of the published build
is set in `src/CashPrism.Shell/CashPrism.Shell.csproj`, never on the publish
command line, so that step is a change to that file. It has to be: a runtime
identifier or a self-contained switch changes which packages are resolved, and
[THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md) is generated from exactly
that set. Passing such a flag on the command line would leave the notices
describing a build nobody distributes.

## Settings

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

## In a container

For a home server or a NAS, CashPrism also runs as a container image, built
from the `Dockerfile` in the repository root:

```sh
docker build -t cashprism .
docker run -d --name cashprism -p 5080:5080 -v cashprism-data:/data -e TZ=Europe/Berlin cashprism
```

Or, as a `compose.yaml` next to wherever you keep it:

```yaml
services:
  cashprism:
    image: cashprism
    ports:
      - "5080:5080"
    volumes:
      - cashprism-data:/data
    environment:
      TZ: Europe/Berlin
    restart: unless-stopped

volumes:
  cashprism-data:
```

What differs from a start on the desktop is set by the image, not by a switch:

- **The data lives in `/data`.** Mount a volume there, or every import is gone
  with the container. The database file is `cashprism.db` in that volume.
- **No browser opens**, and the start banner names the port rather than
  addresses: the addresses inside a container belong to the container network,
  and only the host port it is published on is reachable from another device.
  Type the host's own address and that port.
- **The image runs as a non-root user.** A bind mount instead of a named volume
  therefore has to be writable by user ID 1654.
- **`TZ` decides what "today" is.** Without it the container runs on UTC, and a
  booking late in the evening lands on the next day.

The settings above apply unchanged, as environment variables with `__` in place
of `:` — `Hosting__Port=5099`, for example, which then needs `-p 5099:5099`.

The rule of [one instance per database](#one-instance-per-database) holds across
containers: a second container on the same volume refuses to start. And as on
the desktop, the volume belongs on the machine's local disk, never on a network
share — SQLite's locking is unreliable over SMB and NFS.

The image runs on `linux/amd64` and `linux/arm64`; `docker build` builds it for
the machine it runs on. Inside, it is a portable `dotnet publish -c Release`,
started through `dotnet` rather than the native launcher next to it, so the
build stage never has to target the architecture of the image.

## One instance per database

Only one CashPrism at a time may use a database file. SQLite tolerates
concurrent writers badly, so a second start against the same data directory
refuses with one line naming the file rather than joining in and risking the
data.

The guard is a `cashprism.db.lock` file next to the database, opened exclusively
and held for as long as the process lives. The database file itself cannot carry
the lock, because SQLite has to be able to open it. An operating system closes
the handles of a process that ends, however it ended, so a crash or a pulled
plug leaves nothing to clean up by hand — the next start simply works.

Two instances against two data directories are fine: give the second one its own
`Hosting:DataDirectory` and a different `--port`.

## Why several addresses are printed

More than one line under *From another device* is normal. VPN, Docker and
Hyper-V adapters each add an address, and CashPrism does not guess which one is
yours — type them into the other device until one answers.
