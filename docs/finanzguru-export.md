# The Finanzguru export

What a Finanzguru "Alle Buchungen" export actually looks like, and which of its
properties CashPrism is allowed to rely on.

Everything below was measured on two real exports taken one day apart: 6,324 and
6,327 data rows, covering six years and seven accounts. Two files that close
together are still a thin sample, so the counts are evidence, not a specification
— where a number is quoted it says what was observed, not what Finanzguru
guarantees. Nothing here is derived from Finanzguru documentation; there is none.

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
| C | `Name Referenzkonto` | inline string | 6,324 | The display name that account carries in Finanzguru. Free text, chosen by the account owner. |
| D | `Betrag` | number, format `#,##0.00` | 6,324 | The signed booking amount: negative for spending, positive for income. Two decimal places throughout. |
| E | `Kontostand` | number, format `#,##0.00` | 6,324 | The balance reported for the booking. See [Kontostand is not a running balance](#kontostand-is-not-a-running-balance). |
| F | `Waehrung` | inline string | 6,324 | ISO 4217 currency code. Only one value occurred in the measured export, so a reader must not assume a single currency. |
| G | `Beguenstigter/Auftraggeber` | inline string | 6,324 | The other party of the booking. Free text as the bank transmitted it. |
| H | `IBAN Beguenstigter/Auftraggeber` | inline string | 5,307 | The other party's account. **Not always an IBAN** — see [The IBAN column is not always an IBAN](#the-iban-column-is-not-always-an-iban). |
| I | `Verwendungszweck` | inline string | 5,436 | The payment reference. Free text, up to 248 characters in the measured export, and 92 rows contain a line break. |
| J | `E-Ref` | — | 0 | The SEPA end-to-end reference. **Empty in every single row.** The column exists, it just never carries anything. |
| K | `Mandatsreferenz` | inline string | 1,945 | The SEPA mandate reference. Filled on direct debits, empty otherwise. |
| L | `Glaeubiger-ID` | inline string | 1,945 | The creditor identifier. Filled on exactly the same rows as `Mandatsreferenz`. |
| M | `Analyse-Hauptkategorie` | inline string | 6,324 | Finanzguru's top-level category. A closed catalogue from Finanzguru's point of view, free text from ours. |
| N | `Analyse-Unterkategorie` | inline string | 6,324 | Finanzguru's sub-category. Same caveat. |
| O | `Analyse-Vertrag` | inline string | 6,324 | Whether the booking belongs to a recognised contract. German yes/no words, not a boolean. |
| P | `Analyse-Vertragsturnus` | inline string | 1,257 | How often that contract recurs, as a German interval word. Filled only where `Analyse-Vertrag` says yes. |
| Q | `Analyse-Vertrags-ID` | inline string | 1,257 | Finanzguru's contract identifier: 32 lower-case hexadecimal characters, a UUID without its dashes. Filled on the same rows as `Analyse-Vertragsturnus`. |
| R | `Analyse-Umbuchung` | inline string | 6,324 | Whether the booking is a transfer between two of the owner's own accounts. German yes/no words. |
| S | `Analyse-Vom frei verfuegbaren Einkommen ausgeschlossen` | inline string | 6,324 | Whether Finanzguru leaves the booking out of the freely disposable income. German yes/no words. The longest header in the file at 54 characters. |
| T | `Analyse-Umsatzart` | inline string | 6,265 | How the booking was paid, as a German word from a small closed set — seven values occurred. Blank in 59 rows, which are every row of one single account. |
| U | `Analyse-Betrag` | inline string | 6,324 | Whether the row counts as income or as spending. A German word, redundant with the sign of `Betrag`. |
| V | `Analyse-Woche` | inline string | 6,324 | The calendar week, shaped `YYYY-WW`. |
| W | `Analyse-Monat` | inline string | 6,324 | The month, shaped `YYYY-MM`. |
| X | `Analyse-Quartal` | inline string | 6,324 | The quarter, shaped `YYYY-Qn`. |
| Y | `Analyse-Jahr` | number, no format | 6,324 | The year. The odd one out: the three columns above it are text, this one is a bare number and reads back as `2025.0`. |
| Z | `Buchungs-ID` | inline string | 6,324 | Finanzguru's identifier of the booking: 40 lower-case hexadecimal characters. Distinct in every row, and stable across the two exports — see [What two exports one day apart reveal](#what-two-exports-one-day-apart-reveal). |
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
identifies the other party by their account address, and Finanzguru puts it in
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

### The category catalogue

Both exports carry the same 14 main categories and the same 76 pairs of main
and sub-category, and no row is without either. Finanzguru writes the umlauts
out (`Mobilitaet`, `Rundfunkgebuehren`), so a lookup has to use that spelling.
The catalogue in the app may well be larger; these are the names that occurred.

| `Analyse-Hauptkategorie` | `Analyse-Unterkategorie` |
|---|---|
| `Drogerie` | `Drogerie` |
| `Einnahmen` | `Kapitalertraege`, `Kindergeld`, `Leistungen der Bundesagentur fuer Arbeit`, `Lohn / Gehalt`, `Rente/Pension`, `Sonstige Einnahmen` |
| `Essen & Trinken` | `Lebensmittel`, `Lieferservice`, `Restaurants` |
| `Finanzen` | `Bankgebuehren`, `Kredit`, `Sonstige Finanzausgaben`, `Spende`, `Steuern` |
| `Freizeit` | `Buecher & Zeitungen`, `Gaming`, `In-App-Kaeufe`, `Kino`, `Mitgliedschaft`, `Musik & Podcasts`, `Serien & Filme`, `Sonstige Freizeitausgaben`, `Sport`, `Urlaub`, `Veranstaltungen` |
| `Gesundheit` | `Aerztliche Behandlung`, `Apotheke`, `Sonstige Gesundheitsausgaben` |
| `Haustiere` | `Futter & Tierbedarf`, `Tieraerztliche Behandlung` |
| `Kinder` | `Kinderbetreuung`, `Schule & Foerderung`, `Sonstige Kinderausgaben`, `Taschengeld` |
| `Lifestyle` | `Bekleidung`, `Bildung`, `Cloud-Dienste`, `Elektrohandel`, `Geschenke`, `Mobilfunk`, `Prime-Mitgliedschaft`, `Shopping`, `Sonstiger Lifestyle` |
| `Mobilitaet` | `Auto`, `Bus & Bahn`, `Fahrrad`, `Sharing / Gemietet`, `Tanken`, `Taxi` |
| `Sonstiges` | `Bargeld`, `Kreditkartenabrechnung`, `Sonstige Ausgaben` |
| `Sparen` | `Bausparvertrag`, `Sparen` |
| `Versicherungen` | `Brillenversicherung`, `Gesetzliche Krankenversicherung`, `Haftpflichtversicherung`, `Hausratversicherung`, `KFZ-Versicherung`, `Lebensversicherung`, `Private Krankenversicherung`, `Rechtsschutzversicherung`, `Sonstige Sachversicherung`, `Tierhaftpflichtversicherung`, `Tierkrankenversicherung`, `Unfallversicherung`, `Wohngebaeudeversicherung` |
| `Wohnen` | `Bauen / Renovieren`, `Baufinanzierung`, `Einrichtung`, `Gas`, `Internet & Telefon`, `Rundfunkgebuehren`, `Sonstiges Wohnen`, `Strom` |

Neither "contract" nor "transfer" is a category here. Whether a booking belongs
to a recognised contract is `Analyse-Vertrag`, and whether it moves money
between the owner's own accounts is `Analyse-Umbuchung` — both flags beside the
category, not part of it. Contracts span nearly every main category: the 1,257
contract rows of the second export fall into ten of the fourteen, insurance
first, then children, finance, housing and income.

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
remainder is Finanzguru's own inconsistent labelling of the days around New Year,
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

The German yes/no columns are translated in the same project, by
`FinanzguruFlag`, and translated strictly: a value that is neither word fails and
names the column and the row instead of defaulting to `false`. `ja`/`nein` is a
property of this export rather than of a booking, so the models keep plain
booleans and never see the words. Defaulting would be the expensive kind of
wrong — `Analyse-Umbuchung` alone decides for 916 of 6,327 rows whether they
count as income and spending at all.

`FinanzguruAmount` does the same job for the two money columns. They are
ordinary numeric cells and only the format makes them read as money, so
something has to turn the number into the whole cents CashPrism holds money in;
the conversion goes through `decimal`, because the nearest `double` to an amount
like 1234.56 times 100 lands just beside the whole cent. It is strict for the
same reason `FinanzguruFlag` is: both measured exports carry exactly two decimal
places everywhere, and a third one is a change worth being told about rather
than rounding away.

### What the reader does

`FinanzguruExportReader` puts those pieces together:

- **The worksheet is taken by position.** The name changes with every export, so
  it cannot select anything — but it is parsed, by `FinanzguruSheetName`, because
  it is the only place the export says when it was taken. A name that does not
  match `YYYYMMDD_Export_Alle_Buchungen` is reported rather than guessed at: the
  date decides which version of a booking wins on a re-import, and guessing would
  let a stale export overwrite a newer booking.
- **Three columns come out typed**, because handing them on as text would mean
  taking a decision and hiding it: `Buchungstag` as a `DateTime` with the time
  component kept, `Betrag` and `Kontostand` as whole cents.
- **Everything else comes out as text**, invariant and lossless, which is what a
  stored raw row is built from. The three typed columns appear there too, so the
  record of what the row said stays complete.
- **Reading only.** No deduplication, no persistence, no booking. What a row
  means is the import's decision.

Measured against both real exports, the reader reads every row, finds no unknown
column, converts every amount without a single failure, and finds the time
component on exactly the 52 rows this document counts.

### What the import does

`Importer`, in `CashPrism.Application`, is what turns a read file into stored
data. It never sees a German column name: `FinanzguruImportSource` reads the
file, projects each row onto a `Booking` and renders the row as the JSON a raw
row holds, and hands both on.

- **The projection keeps 23 of the 29 columns.** Besides identity, date,
  amounts, account, party, reference, categories, transfer flag and split, it
  stores how the booking was paid (`Analyse-Umsatzart`), the contract columns
  (`Analyse-Vertrag`, `-Vertragsturnus`, `-Vertrags-ID`), the exclusion from
  the disposable income, `Mandatsreferenz`, `Glaeubiger-ID`, `Tags`, and
  `Kontostand` — under a name that says it is what the export reports, because
  it is [not a running balance](#kontostand-is-not-a-running-balance). Six
  columns are dropped because they carry nothing: the four `Analyse-` period
  columns reproduce from `Buchungstag`, `Analyse-Betrag` repeats the sign of
  `Betrag`, and `E-Ref` is empty in every row. The raw row still holds them.
  `Split-Typ` is translated from `Original` / `Teilbuchung` / `Restbetrag` into
  a role, as strictly as `FinanzguruFlag` translates the yes/no columns, and an
  empty cell is the ordinary case rather than an error.
- **The raw JSON is rendered deterministically** — the export's own column order
  first, unknown columns after it, no indentation. The stored text is compared
  byte for byte to decide whether a booking changed, so a serialiser that
  reordered its keys would make every unchanged row look changed and store the
  whole export again on every import.
- **Three outcomes, none of them an exception.** The file is imported; or its
  hash matches an earlier run and it is reported as already imported, having
  written nothing; or it could not be read and every reason is named at once.
  A file that repeats a `Buchungs-ID` is refused as well — it was distinct in
  every one of 6,324 measured rows, so a repeat breaks the assumption identity
  rests on.
- **A stored booking is replaced only by a later export.** Which of two runs is
  later is decided by the export date from the sheet name, falling back to the
  import time where a name carried none. An older file changes nothing, and its
  rows are not kept either — a raw row is the fallback for what the stored
  projection leaves out, and a row describing a state that is not stored is not
  that.
- **The work is cut into batches of 1,000 bookings.** Each batch looks up only
  the stored states it needs and lets go of them again, because an export always
  carries the owner's complete history and that history only grows. The change
  tracker never holds a whole file, which is where an ORM stops being quick.

What an import did is recorded on the run itself: the file name, its hash, the
sheet name, the export date, and how many rows were read, inserted, updated and
left alone.

## The fixture from a real export

The reader's tests build their workbooks in code, after the same constants the
reader uses, so they cannot notice Finanzguru renaming a column, reordering the
sheet or changing a number format. One file Finanzguru actually wrote closes
that gap:
`src/Tests/CashPrism.Infrastructure.Finanzguru.Tests.Integration/Fixtures/20260907-Export-Alle_Buchungen-anonymised.xlsx`.
It is embedded in the test assembly, read by the reader tests, and its header
row is compared with `FinanzguruColumns.All` in order.

It was produced from the later of the two measured exports with the
[anonymiser](anonymiser.md), from the repository root:

```
dotnet run --project src/Tools/CashPrism.Anonymiser -- <path>/20260907-Export-Alle_Buchungen.xlsx --out src/Tests/CashPrism.Infrastructure.Finanzguru.Tests.Integration/Fixtures --synthetic-values --max-rows 50
```

`--synthetic-values` is what makes it fit for a public repository: every
identity is replaced as usual, and dates and amounts are generated as well.
What stays is Finanzguru's own vocabulary — categories, flags, the currency —
the sheet name with its export date, and the workbook's metadata, which names
Apache POI and the time the export was taken. `--max-rows 50` keeps it at about
19 KB.

**To regenerate it** from a newer export, run the same command with that file,
adding `--force` only to overwrite a file of the same name. The output name
follows the input name, so a different export date means a new file name:
delete the old fixture and update the file name in `Fixtures/RealExport.cs`. `RealExport.DataRowCount` has to match `--max-rows`.
Then run the gates — a failing header test after regenerating is the point of
the fixture, and means the export changed shape.
