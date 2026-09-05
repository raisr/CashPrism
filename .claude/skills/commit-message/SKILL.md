---
name: commit-message
description: Draft a commit message for the pending changes in the CashPrism repo, then commit and push after approval. Use when asked to "commit", "write a commit message", "commit and push", or "draft a commit".
---

# commit-message

Drafts a commit message in this repo's required format, waits for the user's
approval, then commits and pushes. Never commits without explicit approval.

Paths below are relative to the repo root.

## 1. Collect the change set

```bash
bash .claude/skills/commit-message/collect.sh
```

Output gives you:

- `BRANCH:` current branch
- `TICKET:` issue number parsed from the branch name (`feature/<ticket>-...`
  or `fix/<ticket>-...`), or `(none ...)` if there is none
- `SCOPE:` whether the diff shown is the staged set or the whole working tree
- the file list and full diff to base the message on

If `SCOPE` is `working tree (nothing staged)`, decide with the user which files
belong in the commit before staging them.

## 2. Draft the message in this exact format

```
<type>: <summary>[ (#<ticket>)]

- <change one>
- <change two>
```

Rules:

- **First line:** conventional-commit type prefix (`feat:`, `fix:`, `refactor:`,
  `test:`, `docs:`, `chore:`) + concise summary. The whole first line is **max
  50 characters**, issue suffix included. If it is tight, trim the summary, not
  the prefix.
- **Issue suffix:** if `collect.sh` reported a `TICKET`, append ` (#<ticket>)` to
  the first line, e.g. `(#42)`. If it reported `(none ...)`, append nothing — no
  `(#?)` placeholder.
- **Second line:** empty.
- **Body:** one `-` bullet per logical change. Concise and technical. No trailing
  punctuation on any bullet.
- Nothing else: no explanations, no emojis, no `Co-Authored-By`, no session URL,
  no extra sections. `AGENTS.md` is explicit that AI attribution does not belong
  in repo commits — the Git history is the provenance.

Everything in the message is English (repo rule).

## 3. Present and wait

Show the drafted message to the user and ask for approval. Do not proceed on
anything less than a clear yes.

## 4. Commit and push

After approval, stage the agreed files (if not already staged) and:

```bash
git commit -F - <<'EOF'
<the approved message>
EOF
git push
```

Report the short hash and the push result.

## Gotchas

- The repo has `core.autocrlf` behaviour: `git` prints
  `LF will be replaced by CRLF` warnings when staging text files. Harmless —
  not an error, do not try to "fix" it.
- `collect.sh` shows `git diff HEAD` for the unstaged case, which includes both
  staged and unstaged changes so you see the full picture. It does **not** show
  untracked files' contents — only their paths appear in the file list. Read
  untracked files yourself if they are part of the commit.
- Branch `main` never has a ticket, so commits straight to `main` carry no issue
  suffix. That is expected, not a parsing failure.
- The ticket regex accepts a bare number (`42`) or a prefixed key (`PROJ-42`);
  it requires a `-` and a description after the ticket, matching the
  `feature/<ticket>-<short-description>` convention in `AGENTS.md`.
