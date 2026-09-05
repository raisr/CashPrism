#!/usr/bin/env bash
# Pre-flight for a feature/fix branch: confirms you are on a ticket branch,
# shows the ticket and its Definition of Done, then runs the three gates that
# must be green before a commit or PR (build, test, format).
#
# Usage: .claude/skills/implement-feature/preflight.sh
#
# Exit code is 0 only when all three gates pass.
set -uo pipefail

cd "$(git rev-parse --show-toplevel)"

SLN="src/CashPrism.slnx"
fail=0

branch="$(git rev-parse --abbrev-ref HEAD)"
echo "BRANCH: ${branch}"
if [ "${branch}" = "main" ]; then
  echo "  !! on main - create a feature/<issue>-<slug> or fix/<issue>-<slug> branch first"
  fail=1
fi

ticket="$(printf '%s' "${branch}" | sed -nE 's#^(feature|fix)/([A-Za-z]+-[0-9]+|[0-9]+)-.*#\2#p')"
if [ -n "${ticket}" ]; then
  echo "TICKET: #${ticket}"
  echo
  echo "=== ticket ==="
  gh issue view "${ticket}" --json number,title,state,labels \
    -q '"#\(.number) [\(.state)] \(.title)   labels=\(.labels|map(.name)|join(","))"' 2>&1
  echo
  echo "=== Definition of Done (every box must be true before calling it finished) ==="
  gh issue view "${ticket}" --json body -q .body 2>/dev/null \
    | sed -n '/^## Acceptance criteria/,/^## [A-Z]/p' | sed '$d' | sed '/^$/d' \
    || echo "  (no Acceptance criteria section found in the issue body)"
else
  echo "TICKET: (branch name carries no issue number - expected feature/<n>-... or fix/<n>-...)"
  fail=1
fi

echo
echo "=== linked PR ==="
gh pr view "${branch}" --json number,state,isDraft,reviewDecision,url \
  -q '"#\(.number) state=\(.state) draft=\(.isDraft) review=\(.reviewDecision)\n\(.url)"' 2>/dev/null \
  || echo "  none yet"

echo
echo "=== changelog ==="
base="$(git merge-base main HEAD 2>/dev/null || echo HEAD)"
changed="$( { git diff --name-only "${base}...HEAD"; git diff --name-only HEAD; git diff --name-only --cached; } 2>/dev/null | sort -u )"
if printf '%s\n' "${changed}" | grep -q '^src/' && ! printf '%s\n' "${changed}" | grep -qx 'CHANGELOG.md'; then
  echo "  !! src/** changed but CHANGELOG.md was not - add an entry under [Unreleased],"
  echo "     or note in the PR body why it is skipped (see step 4). Warning only, not a gate."
else
  echo "  OK"
fi

run_gate() {
  local label="$1"; shift
  echo
  echo "=== ${label} ==="
  if "$@" > /tmp/preflight-gate.log 2>&1; then
    echo "  PASS"
  else
    echo "  FAIL - last lines:"
    tail -n 15 /tmp/preflight-gate.log | sed 's/^/    /'
    fail=1
  fi
}

run_gate "dotnet build"  dotnet build "${SLN}" --nologo
run_gate "dotnet test"   dotnet test  "${SLN}" --nologo
run_gate "dotnet format" dotnet format "${SLN}" --verify-no-changes

echo
if [ "${fail}" -eq 0 ]; then
  echo "PRE-FLIGHT: all gates green"
else
  echo "PRE-FLIGHT: NOT ready - fix the items above"
fi
exit "${fail}"
