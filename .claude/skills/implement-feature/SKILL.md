---
name: implement-feature
description: Drive a CashPrism change end to end - ticket, branch, implementation in small steps, the build/test/format gates, commit and PR, then the review loop. Use when asked to "implement", "build a feature", "work on issue #N", "fix a bug", "start on the ticket", or "pick up" a piece of work.
---

# implement-feature

The lifecycle for any code change in CashPrism. Nothing here is optional and the
order matters. Paths are relative to the repo root.

Related skills: `create-issue` (step 1), `commit-message` (step 6).

## 1. There must be a ticket

Every change needs a GitHub issue first. If there is none, invoke `create-issue`
and get it filed before writing code. If the user pointed at an existing issue,
read it in full now — the **Acceptance criteria / Definition of Done** is the
contract you are fulfilling.

## 2. Branch from an up-to-date main

```bash
git checkout main && git pull
git checkout -b feature/<issue>-<slug>   # fix/<issue>-<slug> for a fix ticket
```

Slug: lowercase words from the title joined by `-`. Only `feature/` and `fix/`
prefixes — a `chore`/`docs`/`refactor` ticket still gets `feature/<issue>-…` so
the tooling parses the number back out.

## 3. Implement in small, reviewable steps

- Respect the onion rule from `AGENTS.md`: the dependency arrow points inward,
  Infrastructure is referenced only by `Shell`, validation lives in `Domain`.
- New or changed logic without a test counts as unfinished (`AGENTS.md`). Test
  layout mirrors the source path; one test class per class under test, one
  nested class per method. See existing tests under `src/Tests/`.
- Keep the working tree reviewable — no stray files, no reformatted files you
  never touched.
- Anything that touches more than one file: agree the approach with the user
  before diving in.

## 4. Changelog entry

`CHANGELOG.md` in the repo root follows [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/).

- A user-visible change **requires** an entry under `## [Unreleased]`, in the
  right category (`Added` / `Changed` / `Deprecated` / `Removed` / `Fixed` /
  `Security`). Add the category heading if it is not there yet.
- Write it for a human reading the release notes — what changed and why it
  matters, not the commit subject and not a `git log` dump.
- Purely internal work with no user-visible effect (tests, tooling, the skill
  files, CI, doc-only) may skip the entry. When you skip, say so in one line in
  the PR body.

## 5. Pre-flight — all gates green

```bash
bash .claude/skills/implement-feature/preflight.sh
```

Confirms you are on a ticket branch, prints the ticket's Definition of Done, the
linked PR if any, then runs the three gates and exits non-zero unless all pass:

- `dotnet build src/CashPrism.slnx` → 0 warnings, 0 errors
- `dotnet test src/CashPrism.slnx` → green
- `dotnet format src/CashPrism.slnx --verify-no-changes` → clean

Then walk the DoD checkboxes yourself — the gates do not cover reference graphs,
exposed APIs or docs. Only proceed when every box is genuinely true.

## 6. Commit

Invoke `commit-message`. It parses `#<issue>` from the branch, drafts in the
required format, and commits + pushes after your explicit approval. Repo commits
carry **no** AI footer.

## 7. Open the PR

```bash
gh pr create --base main --head <branch> \
  --title "<type>: <summary> (#<issue>)" \
  --label <type> \
  --assignee raisr \
  --body-file /tmp/pr-body.md
```

Body: **what** changed and **why**, an **Evidence** block with the gate output,
a **Changelog** line naming the `CHANGELOG.md` category the entry went under (or
`n. a.` with the reason it was skipped, per step 4), any **deviation from the
ticket's DoD** called out, and `Closes #<issue>`. End with the external-system
signature, separated by `---`:

```
---
🤖 *Claude was here. No hands, but opinions.*
*<YYYY-MM-DD>*
```

Always `--assignee raisr` (add it afterwards with
`gh pr edit <n> --add-assignee raisr` if you forgot).

## 8. Review loop

The user reviews in the PR and comments there, then tells you to look. For each
round:

```bash
gh pr view <n> --json comments,reviews -q '.comments[].body, (.reviews[] | "\(.state): \(.body)")'
gh api repos/raisr/CashPrism/pulls/<n>/comments -q '.[] | "\(.path):\(.line)  \(.body)"'   # line comments
```

Address every point, re-run pre-flight, commit (`commit-message`), push. Reply on
the PR with the commit that resolved each point, signed with the same `---`
block.

## 9. Done

The work is finished only when **the user accepts / merges the PR** — not when
the gates pass. After merge:

```bash
git checkout main && git pull && git branch -d <branch>
```

## Gotchas

- `dotnet test` exits non-zero when a test project has **no** tests. A new test
  project needs at least one real test, not a placeholder.
- Windows is case-insensitive: never `rm` a path that differs from another only
  in case (`src/tests` vs `src/Tests`) — you will delete the wrong one. Use
  `git mv` via a temp name and restore from the index if it bites.
- The repo has `core.autocrlf`; `git` prints `LF will be replaced by CRLF`
  warnings on staging. Harmless. `.gitattributes` normalises to LF in the repo.
- `preflight.sh` runs three full `dotnet` invocations (~30-60 s cold). That is
  expected, not a hang.
- `gh pr view <branch>` only finds the PR while the branch exists locally and on
  the remote; after step 9 use `gh pr view <number>`.
