# The FinanzGuru export

What a FinanzGuru "Alle Buchungen" export actually looks like, and which of its
properties CashPrism is allowed to rely on.

Everything below was measured on two real exports taken one day apart: 6,324 and
6,327 data rows, covering six years and seven accounts. Two files that close
together are still a thin sample, so the counts are evidence, not a specification
— where a number is quoted it says what was observed, not what FinanzGuru
guarantees. Nothing here is derived from FinanzGuru documentation; there is none.

Single-file counts are from the earlier export unless the text says otherwise. No
values from either export are reproduced here. Where a value's shape matters, it
is described as a pattern.

## The workbook

- **One worksheet.** There is no second sheet, no chart sheet, no defined name.
- **The sheet name carries the export date**, in the shape
  `YYYYMMDD_Export_Alle_Buchungen`. It therefore changes with every export, and
  a reader must take the sheet **by position**, never by name.
- **Written by Apache POI**, not by Excel. Two consequences:
  - Every string is stored **inline** in the cell. `xl/sharedStrings.xml` exists
    but is empty (`count="0"`), so a reader that only looks at the shared string
    table sees an empty sheet.
  - The style table is generated per row rather than per format: 12,649 `cellXf`
    entries although the file uses exactly two formats. A writer that rewrites
    the file must not assume the style table is small or shared.
- **Two number formats** are in play: the built-in `4` (`#,##0.00`) on the two
  money columns, and a custom `164` (`dd.MM.yyyy`) on the date column. Both are
  ordinary numeric cells — the format is what makes them read as money and date.
- **The header sits in row 1**, data starts in row 2. There is no title row, no
  frozen pane, no merged cell.
- **29 columns, A to AC**, in the order of the table below.

## The columns

"Cell type" is what the file stores, not what the value means: `inline string`
is a text cell, `number` is a numeric cell, and the format code says how Excel
paints it. "Filled" is the number of the 6,324 data rows that carry a non-empty
value.

| # | Header | Cell type | Filled | Meaning |
|---|---|---|---|---|
| A | `Buchungstag` | number, format `dd.MM.yyyy` | 6,324 | The date the booking was posted. See [Dates are not always whole days](#dates-are-not-always-whole-days). |
| B | `Referenzkonto` | inline string | 6,324 | The account the booking belongs to. Usually an IBAN, but a provider account is identified by its own handle instead. |
| C | `Name Referenzkonto` | inline string | 6,324 | The display name that account carries in FinanzGuru. Free text, chosen by the account owner. |
| D | `Betrag` | number, format `#,##0.00` | 6,324 | The signed booking amount: negative for spending, positive for income. Two decimal places throughout. |
| E | `Kontostand` | number, format `#,##0.00` | 6,324 | The balance reported for the booking. See [Kontostand is not a running balance](#kontostand-is-not-a-running-balance). |
| F | `Waehrung` | inline string | 6,324 | ISO 4217 currency code. Only one value occurred in the measured export, so a reader must not assume a single currency. |
| G | `Beguenstigter/Auftraggeber` | inline string | 6,324 | The other party of the booking. Free text as the bank transmitted it. |
| H | `IBAN Beguenstigter/Auftraggeber` | inline string | 5,307 | The other party's account. **Not always an IBAN** — see [The IBAN column is not always an IBAN](#the-iban-column-is-not-always-an-iban). |
| I | `Verwendungszweck` | inline string | 5,436 | The payment reference. Free text, up to 248 characters in the measured export, and 92 rows contain a line break. |
| J | `E-Ref` | — | 0 | The SEPA end-to-end reference. **Empty in every single row.** The column exists, it just never carries anything. |
| K | `Mandatsreferenz` | inline string | 1,945 | The SEPA mandate reference. Filled on direct debits, empty otherwise. |
| L | `Glaeubiger-ID` | inline string | 1,945 | The creditor identifier. Filled on exactly the same rows as `Mandatsreferenz`. |
| M | `Analyse-Hauptkategorie` | inline string | 6,324 | FinanzGuru's top-level category. A closed catalogue from FinanzGuru's point of view, free text from ours. |
| N | `Analyse-Unterkategorie` | inline string | 6,324 | FinanzGuru's sub-category. Same caveat. |
| O | `Analyse-Vertrag` | inline string | 6,324 | Whether the booking belongs to a recognised contract. German yes/no words, not a boolean. |
| P | `Analyse-Vertragsturnus` | inline string | 1,257 | How often that contract recurs, as a German interval word. Filled only where `Analyse-Vertrag` says yes. |
| Q | `Analyse-Vertrags-ID` | inline string | 1,257 | FinanzGuru's contract identifier: 32 lower-case hexadecimal characters, a UUID without its dashes. Filled on the same rows as `Analyse-Vertragsturnus`. |
| R | `Analyse-Umbuchung` | inline string | 6,324 | Whether the booking is a transfer between two of the owner's own accounts. German yes/no words. |
| S | `Analyse-Vom frei verfuegbaren Einkommen ausgeschlossen` | inline string | 6,324 | Whether FinanzGuru leaves the booking out of the freely disposable income. German yes/no words. The longest header in the file at 54 characters. |
| T | `Analyse-Umsatzart` | inline string | 6,265 | How the booking was paid, as a German word from a small closed set — seven values occurred. Blank in 59 rows, which are every row of one single account. |
| U | `Analyse-Betrag` | inline string | 6,324 | Whether the row counts as income or as spending. A German word, redundant with the sign of `Betrag`. |
| V | `Analyse-Woche` | inline string | 6,324 | The calendar week, shaped `YYYY-WW`. |
| W | `Analyse-Monat` | inline string | 6,324 | The month, shaped `YYYY-MM`. |
| X | `Analyse-Quartal` | inline string | 6,324 | The quarter, shaped `YYYY-Qn`. |
| Y | `Analyse-Jahr` | number, no format | 6,324 | The year. The odd one out: the three columns above it are text, this one is a bare number and reads back as `2025.0`. |
| Z | `Buchungs-ID` | inline string | 6,324 | FinanzGuru's identifier of the booking: 40 lower-case hexadecimal characters. Distinct in every row, and stable across the two exports — see [What two exports one day apart reveal](#what-two-exports-one-day-apart-reveal). |
| AA | `Referenz-Original-ID` | inline string | 2 | The `Buchungs-ID` a split part points back to. See [Split bookings](#split-bookings). |
| AB | `Split-Typ` | inline string | 3 | The role a row plays in a split booking. See [Split bookings](#split-bookings). |
| AC | `Tags` | inline string | 38 | The free-text tags a person put on the booking. |

## Special cases

These are the properties that break a naive reader. Each one was found by
measurement, not by reasoning about what an export ought to look like.

### The IBAN column is not always an IBAN

`IBAN Beguenstigter/Auftraggeber` is filled in 5,307 rows and carries four
distinct shapes:

| Shape | Rows |
|---|---|
| An IBAN | 5,128 |
| A UUID | 68 |
| An **email address** | 52 |
| A short opaque code | 59 |

The email addresses are the PayPal rows: for those bookings the payment provider
identifies the other party by their account address, and FinanzGuru puts it in
the IBAN column unchanged. Anything that validates this column as an IBAN, or
masks it assuming an IBAN, has to cope with all four.

### Dates are not always whole days

52 of 6,324 rows carry a time component: their serial number has a fractional
part, while every other row is a whole day. The cell format is `dd.MM.yyyy`
throughout, so the time is invisible in Excel and only shows up in the stored
value.

Those 52 rows are exactly the 52 rows whose IBAN column holds an email address —
the PayPal bookings again. A reader that compares dates for equality, or hashes
the date into a fingerprint, must decide deliberately whether to keep the time or
drop it; doing nothing silently keeps it.

### `E-Ref` is always empty

The `E-Ref` column exists in every export and was empty in all 6,324 rows. It is
still a **required** column: a column disappearing from the export is a change we
want to notice, and tolerating a missing one because it happened to be empty is
how that change goes unnoticed.

### Split bookings

A booking split into parts appears as several rows, tied together by two columns:

- `Split-Typ` names the role of the row: `Original`, `Teilbuchung` or
  `Restbetrag`.
- `Referenz-Original-ID` holds the `Buchungs-ID` of the `Original` row, on the
  parts that point back at it.

In each measured export this occurs exactly once — three rows, one per role, two
of which carry the back-reference. That is enough to know the mechanism and far
too little to know its edge cases. Both columns are empty on every ordinary row.
The effect on an importer is in [A split booking is double counting waiting to
happen](#a-split-booking-is-double-counting-waiting-to-happen).

### `Kontostand` is not a running balance

`Kontostand` looks like a running balance and is not one. Testing
`Kontostand[i] == Kontostand[i-1] + Betrag[i]` per account, to the cent:

| Account (by size) | Rows | In file order | Sorted by date |
|---|---|---|---|
| 1 | 4,207 | 0 % | 0.1 % |
| 2 | 1,640 | 6.2 % | 22.8 % |
| 3 | 234 | 0.4 % | 22.7 % |
| 4 | 111 | 20.9 % | 80.9 % |
| 5 | 72 | 7.0 % | 62.0 % |
| 6 | 59 | 5.2 % | 5.2 % |

Sorting by date helps on some accounts and not at all on others, and no account
comes close to holding. The column can be shown as what the export claims, but
nothing may be **derived** from it: not a balance history, not a plausibility
check on `Betrag`, not a way to detect missing rows.

## What two exports one day apart reveal

The first export was taken again the next day. Comparing the two booking by
booking is what tells apart what is stable from what is not, and it drives the
import rules in [`../Agents.md`](../Agents.md#overview).

### `Buchungs-ID` is the only stable identity

Every one of the 6,324 rows in the earlier export carries a distinct
`Buchungs-ID`, and every one of them reappears under the same `Buchungs-ID` in
the later export. The later file has three more rows — three bookings added, none
removed, none renumbered.

Nothing else in the export identifies a booking. A fingerprint over date, amount,
currency, account, counterparty and payment reference — the fields a bank
statement is usually deduplicated on — collapses **46 groups** of genuinely
distinct bookings in a single file and silently drops **52** of them. They are
mostly card payments made on the same day to the same merchant for the same
amount: identical on all six fields, distinct only in `Kontostand` and in the
booking reference. The same count came out of both exports.

### Bookings are enriched after the fact

Of the 6,324 bookings present in both exports, **four** differ between the two
files. The columns that moved:

| Column | Bookings changed |
|---|---|
| `Beguenstigter/Auftraggeber` | 4 |
| `IBAN Beguenstigter/Auftraggeber` | 4 |
| `Verwendungszweck` | 4 |
| `Analyse-Hauptkategorie` | 4 |
| `Analyse-Unterkategorie` | 4 |

`Buchungstag` and `Betrag` did not move on any of them. The same `Buchungs-ID`
therefore describes a slightly different row from one export to the next, so a
stored booking is the latest known state, not a fixed record. On a re-import the
later export wins; "later" is the date in the sheet name, and the more recent
import run only when that date cannot be read.

### The four `Analyse-` period columns are a function of `Buchungstag`

`Analyse-Monat`, `Analyse-Quartal` and `Analyse-Jahr` reproduce exactly from
`Buchungstag` — no mismatch in any of the 12,651 rows across both files.
`Analyse-Woche` is a Sunday-anchored week count (week 1 is 1 January to the first
Saturday, a new week every Sunday); it reproduces about 99 % of rows, and the
remainder is FinanzGuru's own inconsistent labelling of the days around New Year,
where late December is variously tagged `YYYY-01`, `YYYY-52` or `YYYY-53`.

None of the four carries anything `Buchungstag` does not. A change-detection diff
between two exports can ignore them: they cannot move unless `Buchungstag` moves.

### A split booking is double counting waiting to happen

Each export contains exactly one [split booking](#split-bookings): an `Original`
row plus a `Teilbuchung` and a `Restbetrag` row, and the two parts' `Betrag` add
up to the `Original` `Betrag` to the cent. Any sum over the `Betrag` column
counts that booking twice. One split in 6,324 rows makes the effect tiny here,
but an importer that treats every row as a booking over-counts both the
transaction count and every amount total by the split rows — the factor is one
extra full copy of each split booking.

### Storing every raw row is expensive and buys nothing

Serialised as a JSON object of all 29 columns, one row is **912 bytes**. A full
daily import is 5.5 MB of raw rows; a year of them is about **2 GB** to describe
roughly 7,400 bookings. Keeping only the rows that are new or changed since the
last import costs 6,324 rows once and then a handful per day — 7 on the measured
second day — for single-digit MB a year, and loses nothing, because an unchanged
row is byte-identical to the one already stored. The `.xlsx` file itself, about
1.2 MB per export, is not kept either.

## What CashPrism does with this

Columns are resolved **by header name, never by index** — see
`FinanzguruColumnMap` in `src/CashPrism.Infrastructure.Finanzguru`. The export
comes from a third party we do not control, and a column silently shifting by one
would make a writer overwrite the wrong column, which is the most expensive way a
tool like this can fail.

Three rules follow from that:

- **A missing known column fails**, naming the column. All 29 are required,
  `E-Ref` included.
- **A known column appearing twice fails**, naming it. Which of the two to read
  is undecidable, and guessing is exactly the failure above.
- **An unknown extra column is reported, not judged.** The caller decides what it
  means — the anonymiser refuses to touch a file it does not fully understand,
  the importer passes the column through. Keeping that policy out of the mapping
  is what lets both share it.

The mapping takes the header row as a plain list of strings and returns the
name-to-index assignment. It reads no file and mentions no ClosedXML type, so it
is usable from a tool that takes no spreadsheet dependency at all.
