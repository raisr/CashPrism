# The anonymiser

`CashPrism.Anonymiser` is a standalone console tool: it turns a real Finanzguru
export into one that is safe to share — as an issue attachment, in this
repository's own fixtures, wherever a real export must not go. It is a
development tool, not part of the shipped application; nothing in `Shell`
depends on it.

## Command line

```
CashPrism.Anonymiser <input.xlsx> [<input2.xlsx> …] --out <directory>
    [--force] [--scale <factor>] [--max-rows <n>] [--synthetic-values]
```

- One or more input files, in any order relative to the options.
- `--out <directory>` is mandatory and has no default. An anonymised file must
  never be written next to the real one by accident.
- `--force` allows overwriting an existing output file. Without it, an
  existing file aborts the run.
- `--scale <factor>` multiplies `Betrag` and `Kontostand` together by
  `<factor>`, rounded to two decimals. Defaults to `1.0` — unchanged. A
  factor that is not a positive number aborts the run.
- `--max-rows <n>` keeps only the newest `n` data rows per file — a plain
  prefix, since the export's row order is already newest first. Without it,
  every row is kept. The limit is applied before the replacement, so
  placeholder numbers stay dense instead of leaving gaps for values that only
  occurred in a dropped row.
- `--synthetic-values` replaces the dates and amounts as well — see
  [Synthetic values](#synthetic-values). Off by default.

The switches combine freely with each other and with `--force`. With
`--synthetic-values`, `--scale` applies to the generated amounts.

The output file is the input name with `-anonymised` inserted before the
extension, e.g. `export.xlsx` → `export-anonymised.xlsx`.

Every input file of one run shares its value dictionaries (see below): the
same counterparty keeps the same placeholder whether it appears in one file or
in two overlapping exports. All input files are read and validated before any
output is written, since the dictionaries have to see every file first.

## Why it does not use ClosedXML

An `.xlsx` is a zip archive. ClosedXML would rewrite the whole workbook on the
way through — inline strings become shared strings, the per-row stylesheet a
real Finanzguru export carries (see
[finanzguru-export.md](finanzguru-export.md)) disappears — and the result no
longer resembles what Finanzguru itself produces. A parser bug in reading
inline strings, the only string form Finanzguru actually emits, would then stay
invisible in every test built from an anonymised fixture and surface for the
first time on a real import.

The tool therefore copies every zip entry through unchanged, byte for byte,
except the one worksheet part it rewrites. Even there, only the text inside a
replaced cell changes — every other byte, including the cells of every kept
column, is untouched.

## What gets replaced

| Col | Header | Rule |
|---|---|---|
| A | `Buchungstag` | kept; generated with `--synthetic-values` |
| B | `Referenzkonto` | replaced, shape-preserving |
| C | `Name Referenzkonto` | replaced → `Account 01` |
| D | `Betrag` | kept, optionally scaled via `--scale`; generated with `--synthetic-values` |
| E | `Kontostand` | kept, optionally scaled via `--scale`; generated with `--synthetic-values` |
| F | `Waehrung` | kept |
| G | `Beguenstigter/Auftraggeber` | replaced → `Counterparty 001` |
| H | `IBAN Beguenstigter/Auftraggeber` | replaced, shape-preserving |
| I | `Verwendungszweck` | replaced → `Reference 0001` |
| J | `E-Ref` | kept (empty throughout) |
| K | `Mandatsreferenz` | replaced |
| L | `Glaeubiger-ID` | replaced |
| M–N | `Analyse-Hauptkategorie`, `-Unterkategorie` | kept |
| O–P | `Analyse-Vertrag`, `-Vertragsturnus` | kept |
| Q | `Analyse-Vertrags-ID` | replaced |
| R–U | `-Umbuchung`, `-Vom frei verfuegbaren Einkommen ausgeschlossen`, `-Umsatzart`, `-Betrag` | kept |
| V–Y | `Analyse-Woche`, `-Monat`, `-Quartal`, `-Jahr` | kept; recomputed from the generated date with `--synthetic-values` |
| Z | `Buchungs-ID` | replaced |
| AA | `Referenz-Original-ID` | replaced through the same dictionary as `Buchungs-ID` |
| AB | `Split-Typ` | kept |
| AC | `Tags` | replaced |

Categories are kept because without them the file is nothing but parser feed —
an anonymised export still has to exercise the rest of the application.
`Buchungs-ID` and `Referenz-Original-ID` are harmless in themselves but
identify real bookings at Finanzguru, and replacing them costs nothing.

## How a value is replaced

Every replaced column draws from one of a handful of dictionaries, each built
once per run from every input file:

- `Referenzkonto` and `IBAN Beguenstigter/Auftraggeber` share one dictionary,
  so an own IBAN that also shows up as a counterparty IBAN gets the same
  replacement in both places — a transfer between two of the owner's own
  accounts stays recognisable as one.
- `Name Referenzkonto` and `Beguenstigter/Auftraggeber` share a second
  dictionary the same way.
- `Buchungs-ID` and `Referenz-Original-ID` share a third: a split booking's
  back-reference still points at the same (now anonymised) row.
- `Verwendungszweck`, `Mandatsreferenz`, `Glaeubiger-ID`, `Analyse-Vertrags-ID`
  and `Tags` each get their own dictionary.

A dictionary assigns its placeholders in two steps: every distinct value it
will ever hold is collected first, then sorted ordinally, then numbered in
that order. Nothing here depends on which row a value happened to sit in, or
on the order two exports place their rows in — the same input always produces
the same output, with no salt and nothing to keep secret. The `Referenzkonto`
/ `Name Referenzkonto` values are collected and numbered before everything
else, so the owner's own few accounts always claim the lowest numbers even
when a hundred counterparties would otherwise sort ahead of them.

`Referenzkonto` and `IBAN Beguenstigter/Auftraggeber` are shape-preserving: a
value that looks like an IBAN becomes an IBAN-shaped value of the same length,
and a value that looks like an email address — the PayPal rows put one in this
column — becomes `account-01@example.invalid`. Anything else keeps its length
with a numeric placeholder in it, honest about not knowing its shape rather
than guessing one.

## The self-check

After writing a file, the tool reads it straight back and checks, column by
column, that none of a replaced column's original values are still in there
anywhere in the output. If one is, the run aborts and **deletes the file it
just wrote** — a file with forgotten cleartext looks exactly like a finished
one otherwise, and this is the only mechanism that turns "we replaced it" into
a checked fact rather than a claim.

## Synthetic values

Without `--synthetic-values`, the output still tells how the household spends:
dates, amounts and balances are the real ones. That is fine for a file shared
privately. A fixture in a public repository must not carry them, and the switch
is for that case.

The values are **generated, not transformed**. The tool has no salt and nothing
secret, so a constant offset or factor would be undone by anyone who reads its
source. Every value is computed from the row it sits in, never from the value
it replaces:

- **`Buchungstag`:** the first data row gets 1 January 2001, each further row
  one day earlier. That is before Finanzguru existed, so no generated date is a
  real one, and the export's newest-first order holds. A row with a time of day
  keeps it — the fractional part of the serial number is copied digit for
  digit.
- **`Betrag` and `Kontostand`:** a value derived from the row number — up to
  999.99 for `Betrag`, up to 99,999.99 for `Kontostand` — with the original's
  sign, so `Analyse-Betrag` still agrees. A zero stays zero: it says nothing
  about anybody. The original of a split booking gets the sum of its generated
  parts, so the parts still add up to it to the cent.
- **`Analyse-Woche`, `-Monat`, `-Quartal`, `-Jahr`:** recomputed from the
  generated date, so the invariant described in
  [finanzguru-export.md](finanzguru-export.md) still holds.

Only the text inside a replaced cell changes; its style and type stay, and the
numbers are printed the way the export prints them. Categories and flags are
kept: they are Finanzguru's fixed vocabulary, not the owner's data.

A second self-check runs with the switch. It compares every date, amount and
balance of the written file with the cell it replaced and, as with the first
one, deletes the file and fails the run when a non-zero value is still the
real one.

## What this version does not do

- **Generated IBANs and creditor-style identifiers carry no valid check
  digit.** Deliberate: a valid one would tempt a future check-digit validation
  into trusting the placeholder as bookable data, and any such validation
  added later should reject this data as what it is — anonymised, not real.
- **No protection against re-identification from the retained columns** unless
  `--synthetic-values` is given. Without it, dates, amounts, balances and
  categories stay as they are; anyone reading the file still sees how the
  household spends. The tool removes identities, not information.
- **`--scale` is cosmetic, not a safeguard.** A constant factor hides the
  absolute level but leaves every relation intact — rent against income, loan
  against savings still divide out to the same ratios they did in the real
  file.
- **`--max-rows` is not coverage.** "The newest `n` rows" is a plain prefix,
  not a sample. In the measured export (see
  [finanzguru-export.md](finanzguru-export.md)) the 3 split rows and the 52
  rows with a time component sit scattered through six years of history; a
  small prefix will most likely contain neither. A fixture that needs those
  cases needs a coverage-preserving selection, which this tool does not do.
