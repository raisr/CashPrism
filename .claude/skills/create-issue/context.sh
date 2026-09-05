#!/usr/bin/env bash
# Gathers the context needed to draft a new issue in the CashPrism schema:
#   - that gh is authenticated and which repo it will file against
#   - the conventional-commit type labels that exist (title prefix == label)
#   - open issues, so a near-duplicate can be spotted before filing
#
# Usage: .claude/skills/create-issue/context.sh
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

echo "=== auth ==="
gh auth status 2>&1 | sed -n '1,4p'

echo
echo "=== repo ==="
gh repo view --json nameWithOwner,url -q '.nameWithOwner + "  " + .url'

echo
echo "=== type labels (use one as --label AND as the title prefix) ==="
have=""
for t in feat fix refactor docs chore; do
  if gh label list --json name -q '.[].name' | grep -qx "$t"; then
    echo "  $t"
    have="$have $t"
  fi
done
missing=""
for t in feat fix refactor docs chore; do
  case " $have " in *" $t "*) ;; *) missing="$missing $t";; esac
done
if [ -n "$missing" ]; then
  echo "  MISSING:$missing"
  echo "  create with: gh label create <name> --color <hex> --description \"conventional commit: <name>\""
fi

echo
echo "=== open issues (dedup check) ==="
gh issue list --state open --json number,title,labels \
  -q '.[] | "  #\(.number) [\(.labels | map(.name) | join(","))] \(.title)"'
