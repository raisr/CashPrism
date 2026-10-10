# Releasing

How a version of CashPrism is released: what to do by hand, what
`.github/workflows/release.yml` does with it, and what makes it stop.

A release is a version tag on `main`, created by hand. Nothing is released on
merge, because a release publishes: the downloads on GitHub and the container
image on GHCR. The version comes from the tag and nowhere else; every build
from source carries `0.0.0-dev`.

## What the workflow does

Pushing a tag `v*` starts it. In order:

1. **Checks the tag.** It must point at a commit on `main` (or on a
   `release/X.Y` branch, see [A fix to an old release](#a-fix-to-an-old-release))
   and read `vX.Y.Z` or `vX.Y.Z-rc.N`. For `vX.Y.Z`, `CHANGELOG.md` must have a
   `## [X.Y.Z]` section; that section becomes the release text.
2. **Runs the gates** — the same workflow as on a pull request, both operating
   systems and both image architectures, on the tagged commit.
3. **Builds every platform** listed in `RuntimeIdentifiers` of
   `src/CashPrism.Shell/CashPrism.Shell.csproj`, with `-p:Version` from the tag.
   macOS is built on a macOS runner, where the SDK signs the executable ad hoc;
   without that signature it does not start on Apple silicon. Each build is
   packed as `cashprism-<version>-<rid>.zip` (Windows) or `.tar.gz`.
4. **Creates the GitHub release** with the archives, `demo-export.xlsx` and a
   `SHA256SUMS` over all of them. The text is the changelog section followed by
   [`.github/release-notes-footer.md`](../.github/release-notes-footer.md):
   checksums, the SmartScreen and Gatekeeper warnings, Smart App Control.
5. **Pushes the container image** `ghcr.io/raisr/cashprism` for `linux/amd64`
   and `linux/arm64`, tagged with the version and, for a release from `main`,
   `latest`.

| Tag | Changelog section | GitHub release | Image tags |
|---|---|---|---|
| `vX.Y.Z` on `main` | required | published, marked latest | `X.Y.Z`, `latest` |
| `vX.Y.Z` on `release/X.Y` | required | published, not marked latest | `X.Y.Z` |
| `vX.Y.Z-rc.N` | not needed; `[Unreleased]` is shown | draft | `X.Y.Z-rc.N` |

## A release candidate

A release candidate checks the whole pipeline before anything can be
downloaded: the GitHub release stays a draft. Tag the current `main`:

```sh
git checkout main
git pull
git tag -a v0.1.0-rc.1 -m "CashPrism 0.1.0-rc.1"
git push origin v0.1.0-rc.1
gh run watch "$(gh run list --workflow release.yml --limit 1 --json databaseId --jq '.[0].databaseId')"
```

Then look at the draft under *Releases*, download an archive, start it, and run
the image:

```sh
docker run --rm -p 5080:5080 ghcr.io/raisr/cashprism:0.1.0-rc.1
```

Unlike the release, the image has no draft: the package takes the visibility of
the public repository, so anyone can pull a candidate's tag. A candidate that is not needed any more is removed
with its tag:

```sh
gh release delete v0.1.0-rc.1 --cleanup-tag --yes
```

## A release

1. **Cut the changelog** in a pull request that changes nothing else:
   - `## [Unreleased]` becomes `## [X.Y.Z] - YYYY-MM-DD`,
   - a new, empty `## [Unreleased]` goes above it,
   - the compare links at the bottom gain the new version:

     ```markdown
     [Unreleased]: https://github.com/raisr/CashPrism/compare/vX.Y.Z...HEAD
     [X.Y.Z]: https://github.com/raisr/CashPrism/compare/vPREVIOUS...vX.Y.Z
     ```

     The first release links to `https://github.com/raisr/CashPrism/releases/tag/vX.Y.Z`
     instead, as there is nothing to compare with.

2. **Merge it, then tag `main`:**

   ```sh
   git checkout main
   git pull
   git tag -a vX.Y.Z -m "CashPrism X.Y.Z"
   git push origin vX.Y.Z
   ```

The workflow fails before building anything if the section is missing — cut
the changelog, merge, and push the tag again after deleting it with
`git push origin :refs/tags/vX.Y.Z` and `git tag -d vX.Y.Z`.

## A fix to an old release

Only when a fix has to reach a version that is no longer the latest: branch
`release/X.Y` from the last tag of that line, bring the fix onto it, add a
`## [X.Y.Z]` section to its `CHANGELOG.md`, and tag it there:

```sh
git checkout -b release/0.1 v0.1.0
git push -u origin release/0.1
# bring the fix onto the branch, add the changelog section, commit, push
git tag -a v0.1.1 -m "CashPrism 0.1.1"
git push origin v0.1.1
```

The release is published but not marked as the latest, and the image does not
move `latest`. The fix goes to `main` as well, through the usual pull request.

## Screenshots for the user guide

[`benutzung.md`](benutzung.md) shows one image per use case, kept in
[`images/benutzung/`](images/benutzung/). A pull request that visibly changes
one of those pages takes the image again; before a release, look through them
once against the build being released.

Every image comes from the same starting point, so they agree with each other
and show nothing but invented data:

1. Start CashPrism from source on an empty data directory of its own, so no
   real export is anywhere near it:

   ```sh
   dotnet run --project src/CashPrism.Shell -- --port 5099 --Hosting:DataDirectory=<empty directory>
   ```

2. In the browser, open `http://localhost:5099` in a window whose page area is
   1280×800, in the light theme and with the German interface.
3. Take `passwort-festlegen.png` on the empty setup page, set a password with
   the setup code from the console, take `anmelden.png` on the empty sign-in
   page, and sign in.
4. Import [`samples/demo-export.xlsx`](../samples/demo-export.xlsx) and take
   the rest:

| Image | Page and state |
|---|---|
| `import.png` | *Import* right after the import, with *Fertig!* and the four numbers |
| `bisherige-importe.png` | *Import* reloaded, scrolled down to *Bisherige Importe* |
| `navigation.png` | *Übersicht*, the pointer resting on *Analyse* in the navigation |
| `uebersicht.png` | *Übersicht*, the pointer on a point of the chart so its tooltip shows |
| `analyse.png` | *Analyse* with *6 Monate* |
| `buchungsliste.png` | *Buchungen* with the details of one booking open |
| `erreichbar.png` | *Einstellungen*, the address under *Erreichbar unter* replaced by `http://192.168.1.7:5080` in the browser's developer tools |
| `alle-daten-loeschen.png` | *Einstellungen* after *Alle Daten löschen*, showing the question — not confirmed — with the address replaced as above |
| `passwort-aendern.png` | *Passwort ändern*, empty |

The address in `erreichbar.png` and `alle-daten-loeschen.png` is replaced
because the real one belongs to the machine that took the picture; the
replacement is the one the user guide uses for the console. Each image is a PNG
of the page area only, at most 300 KB.

## What is not signed

The binaries carry no code signature beyond the ad-hoc one macOS requires.
Windows SmartScreen warns and can be clicked past; macOS asks for an explicit
*Open Anyway*. Windows Smart App Control blocks an unsigned program outright,
with no way past it for a single program, so on such a machine CashPrism does
not start. The release text and the user guide say so; the container image is
the way around it.
