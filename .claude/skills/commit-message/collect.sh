#!/usr/bin/env bash
# Collects the raw material for drafting a commit message:
#   - current branch and the ticket number parsed from it
#   - the change set (staged if anything is staged, otherwise the full working tree)
#
# Usage: .claude/skills/commit-message/collect.sh
#
# Ticket parsing follows the branch convention in Agents.md:
#   feature/<ticket>-<desc>  or  fix/<ticket>-<desc>
# where <ticket> is either a bare number (42) or PREFIX-number (PROJ-42).
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

branch="$(git rev-parse --abbrev-ref HEAD)"
echo "BRANCH: ${branch}"

ticket="$(printf '%s' "${branch}" | sed -nE 's#^(feature|fix)/([A-Za-z]+-[0-9]+|[0-9]+)-.*#\2#p')"
if [ -n "${ticket}" ]; then
  echo "TICKET: ${ticket}"
else
  echo "TICKET: (none - omit the issue suffix)"
fi

if ! git diff --cached --quiet; then
  scope="staged"
else
  scope="working tree (nothing staged)"
fi
echo "SCOPE: ${scope}"

echo
echo "=== files ==="
if [ "${scope}" = "staged" ]; then
  git diff --cached --stat
else
  git status --short
fi

echo
echo "=== diff ==="
if [ "${scope}" = "staged" ]; then
  git diff --cached
else
  git diff HEAD
fi
