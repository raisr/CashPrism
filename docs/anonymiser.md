# The anonymiser

`CashPrism.Anonymiser` is a standalone console tool: it turns a real FinanzGuru
export into one that is safe to share — as an issue attachment, in this
repository's own fixtures, wherever a real export must not go. It is a
development tool, not part of the shipped application; nothing in `Shell`
depends on it.

## Command line

```
CashPrism.Anonymiser <input.xlsx> [<input2.xlsx> …] --out <directory> [--force]
```

- One or more input files, in any order relative to the options.
- `--out <directory>` is mandatory and has no default. An anonymised file must
  never be written next to the real one by accident.
- `--force` allows overwriting an existing output file. Without it, an
  existing file aborts the run.

The output file is the input name with `-anonymised` inserted before the
extension, e.g. `export.xlsx` → `export-anonymised.xlsx`.

## Why it does not use ClosedXML

An `.xlsx` is a zip archive. ClosedXML would rewrite the whole workbook on the
way through — inline strings become shared strings, the per-row stylesheet a
real FinanzGuru export carries (see
[finanzguru-export.md](finanzguru-export.md)) disappears — and the result no
longer resembles what FinanzGuru itself produces. A parser bug in reading
inline strings, the only string form FinanzGuru actually emits, would then stay
invisible in every test built from an anonymised fixture and surface for the
first time on a real import.

The tool therefore copies every zip entry through unchanged, byte for byte.
Only the worksheet part is read first — to resolve its header against the same
`FinanzguruColumnMap` the real parser uses, and to check that it does not use
shared strings, which this tool does not support.

## What this version does — and does not — do

This first version proves the round trip: an input file comes out unchanged,
byte for byte, entry for entry. It aborts rather than guesses when:

- the worksheet uses shared strings instead of inline strings,
- the header row is missing a known column, carries one more than once, or
  carries a column the mapping does not know,
- the output file already exists and `--force` was not given.

No value is replaced yet. Assigning anonymised placeholders — sequential,
shape-preserving, consistent across a run — is the next step in `ROADMAP.md`'s
M2 phase.
