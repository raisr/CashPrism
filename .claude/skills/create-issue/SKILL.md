---
name: create-issue
description: Draft a GitHub issue for the CashPrism repo in the required schema, then create it with gh after approval. Use when asked to "create an issue", "open a ticket", "file a ticket", "new issue", "draft a ticket", or before starting any change that has no ticket yet.
---

# create-issue

Every change to CashPrism needs a ticket first. This skill drafts one in the
repo's fixed schema, shows it, waits for an explicit yes, then files it with
`gh` and reports the issue number and the branch name to use.

Paths below are relative to the repo root.

## 1. Gather context

```bash
bash .claude/skills/create-issue/context.sh
```

Gives you: that `gh` is authenticated and against which repo, the
conventional-commit type labels that exist, and the list of open issues so you
can spot a near-duplicate before filing. If the script reports `MISSING:` labels
and you need one, create it with the `gh label create` line it prints.

## 2. Draft the issue in this exact schema

**Title:** `<type>: <short imperative>` — `<type>` is one of `feat` `fix`
`refactor` `docs` `chore` (same as the `--label`).

**Body:**

```markdown
## Description

What should be done, and why. Only what is known up front — no speculation,
no "we might also need…". Facts and already-taken decisions only.

## Non-goals

One or two lines on what is deliberately out of scope for this ticket.

## Acceptance criteria / Definition of Done

- [ ] Each item verifiable by both the user and the agent.
- [ ] Where possible a concrete command with its expected result, e.g.
      `dotnet build src/CashPrism.slnx` → 0 warnings, 0 errors.
- [ ] Prefer checkable facts over prose ("X file exists and contains Y",
      not "X is set up correctly").

## Type

`<type>`

---
🤖 *Claude was here. No hands, but opinions.*
*<YYYY-MM-DD>*
```

Rules:

- **English only** (repo rule), even though the conversation is German.
- **Only up-front knowledge.** Things discovered while working the ticket do not
  get retro-fitted into the description later — they go into a ticket comment or
  a follow-up issue.
- **Keep it simple.** No estimates, no assignees, no labels beyond the type, no
  milestone unless the user asks.
- The trailing signature block is required — a GitHub issue is an external system
  (`Agents.md` → "Signing AI-generated content"). Use today's date, `YYYY-MM-DD`.

## 3. Present and wait

Show the full title and body in the chat. Do not create anything on less than a
clear yes. If the user wants changes, redraft and show again.

## 4. Create the issue

Write the approved body to a temp file, then:

```bash
gh issue create \
  --title "<type>: <summary>" \
  --label "<type>" \
  --body-file /tmp/issue-body.md
```

`gh` prints the new issue URL.

## 5. Report back

- The issue number and URL.
- The branch name to use: `fix/<n>-<slug>` for a `fix` ticket, otherwise
  `feature/<n>-<slug>`. Derive the slug from the summary:

  ```bash
  echo "scaffold the Visual Studio solution structure" \
    | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-//;s/-$//'
  ```

  Only `feature/` and `fix/` prefixes are used, so that the `commit-message`
  skill's ticket parser picks the number back up. A `chore`/`docs`/`refactor`
  ticket still gets a `feature/<n>-…` branch.

## Gotchas

- The five type labels (`feat` `fix` `refactor` `docs` `chore`) were created for
  this workflow and are **not** GitHub's defaults. The stock `enhancement` /
  `bug` / `documentation` labels still exist — do not use them for the type;
  they carry no conventional-commit meaning here.
- `context.sh` needs a network round-trip to GitHub; it is not offline-safe.
- `gh issue create` with `--body-file -` reads stdin, but a here-doc there is
  fragile with Markdown backticks — write a real temp file and pass its path.
- Branch `main` carries no ticket by design; this skill is about the issues that
  feed the feature/fix branches, not about `main`.
