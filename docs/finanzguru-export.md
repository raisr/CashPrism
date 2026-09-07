# The FinanzGuru export

What a FinanzGuru "Alle Buchungen" export actually looks like, and which of its
properties CashPrism is allowed to rely on.

Everything below was measured on one real export: a single file of 6,324 data
rows covering six years and seven accounts. One file is a thin sample, so the
counts are evidence, not a specification — where a number is quoted it says what
was observed, not what FinanzGuru guarantees. Nothing here is derived from
FinanzGuru documentation; there is none.

No values from that export are reproduced here. Where a value's shape matters,
it is described as a pattern.

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
| Z | `Buchungs-ID` | inline string | 6,324 | FinanzGuru's identifier of the booking: 40 lower-case hexadecimal characters. Distinct in every row of the measured export. |
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

In the measured export this occurs exactly once — three rows, one per role, two
of which carry the back-reference. That is enough to know the mechanism and far
too little to know its edge cases. Both columns are empty on every ordinary row.

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
