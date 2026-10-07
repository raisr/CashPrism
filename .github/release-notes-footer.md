---

## Before you start

Download the archive for your platform, unpack it, and start `CashPrism.Shell`
(`CashPrism.Shell.exe` on Windows) inside it. How to use CashPrism is described,
in German, in [docs/benutzung.md](https://github.com/raisr/CashPrism/blob/v{{VERSION}}/docs/benutzung.md).

- **Check the download** against `SHA256SUMS`: `sha256sum -c SHA256SUMS --ignore-missing`
  on Linux, `shasum -a 256 -c SHA256SUMS --ignore-missing` on macOS,
  `Get-FileHash <archive>` in PowerShell on Windows and compare by eye.
- **The binaries are not signed.** Windows SmartScreen warns on the first start:
  *More info* → *Run anyway*. macOS refuses to open it until it is allowed under
  *System Settings* → *Privacy & Security* → *Open Anyway*, or until
  `xattr -dr com.apple.quarantine <folder>` is run on the unpacked folder.
- **Windows Smart App Control blocks unsigned programs with no way past it.**
  On a machine where it is on, CashPrism does not start. Run the container image
  on another machine in the house instead.
- **Linux** needs ICU (`libicu`), which desktop distributions have installed.

`demo-export.xlsx` is a made-up Finanzguru export to try CashPrism with.

The container image, for a home server or a NAS:

```sh
docker run -d -p 5080:5080 -v cashprism-data:/data ghcr.io/raisr/cashprism:{{VERSION}}
```
