# The demo data generator

`CashPrism.DemoData` is a standalone console tool that writes a fully synthetic
Finanzguru export: three years of a fictional household's bookings, for trying
CashPrism without an export of one's own. It is a development tool, not part of
the shipped application; nothing in `Shell` depends on it.

Unlike the [anonymiser](anonymiser.md), it reads nothing. No value of a real
export enters it, not even as a template — what it shares with a real export is
the shape, as [`finanzguru-export.md`](finanzguru-export.md) measured it.

## Command line

```
CashPrism.DemoData --out <directory> [--until <yyyy-MM-dd>]
```

- `--out <directory>` is mandatory. The directory is created where it is
  missing, and the file is always called `demo-export.xlsx`; an existing file
  of that name is replaced, since it is generated and can be generated again.
- `--until <yyyy-MM-dd>` is the last day the export covers and its export date.
  It defaults to today. The worksheet is named after it, in the shape a real
  export uses: `--until 2026-10-01` gives `20261001_Export_Alle_Buchungen`.

The same `--until` always gives the same cells. The random number generator
runs on a fixed seed, and every identifier is derived from a hash rather than
drawn. Only the bytes of the file differ between two runs, because the zip
container records when it was written.

## What the data covers

One household — two adults with a joint account, a child with pocket money —
over the three years that end on `--until`, about 1,900 rows. Every person,
merchant, IBAN and email address is made up. IBANs use the bank code `99950000`
and carry a correct check number; email addresses end in `.example`.

Seven accounts, in the three shapes `Referenzkonto` takes in a real export:

| Account | `Referenzkonto` | What happens on it |
|---|---|---|
| Girokonto | IBAN | Salary, child benefit, contracts, card payments, cash |
| Gemeinschaftskonto | IBAN | Groceries and household spending by card |
| Tagesgeld | IBAN | Monthly savings in, quarterly interest |
| Kreditkarte | UUID | Online shopping, public transport, a summer holiday |
| Immobiliendarlehen | IBAN | The monthly mortgage instalment |
| Altes Sparkonto | IBAN | Four fees, then emptied into the Tagesgeld; silent ever after |
| Paynet | handle | Payments to parties named by email address |

The special cases the measurements found are covered deliberately, each by one
test in `CashPrism.DemoData.Tests.Unit`:

- **Categories** come only from the measured catalogue, umlauts written out.
- **Contracts** at every interval — `monatlich`, `zweimonatlich`,
  `vierteljaehrlich`, `halbjaehrlich`, `jaehrlich` — each with a 32-hex
  `Analyse-Vertrags-ID`. Salary and child benefit are income contracts, and a
  streaming subscription ends fourteen months before the export.
- **Transfers** between the household's own accounts come as pairs: the same
  day, the same amount with opposite signs, both flagged in
  `Analyse-Umbuchung`. That covers the savings, the joint account, the
  mortgage, the credit card settlement and the payment provider's top-ups.
- **One booking is excluded from the disposable income without being a
  transfer** — a furniture purchase — so the two flags disagree once.
- **One split booking**: an `Original`, a `Teilbuchung` and a `Restbetrag`,
  the two parts pointing back at the original and adding up to it.
- **Card payments** carry an ISO timestamp at the start of the payment
  reference.
- **Direct debits**, and only they, carry a mandate reference and a creditor ID.
- **The payment provider** names the other party by email address in the IBAN
  column, and exactly those rows carry a time of day in `Buchungstag`. Its
  top-ups leave `Analyse-Umsatzart` blank.
- **Tags** on a few rows: the yearly donation, the holiday, a gift.
- **The period columns** follow from `Buchungstag` by the measured rules, and
  `Analyse-Jahr` is a number, not text.
- **`E-Ref`** is empty in every row, and every row has its own 40-hex
  `Buchungs-ID`.

`Kontostand` is a running balance per account here, which a real export's is
not. Nothing in CashPrism may derive anything from that column either way, so
the difference does not matter.

## The file

The workbook is written with ClosedXML. It does not imitate the Apache POI
internals of a real export — inline strings, a style per row — because the
reader reads both. For a file Finanzguru itself wrote, see the fixture in
[`finanzguru-export.md`](finanzguru-export.md#the-fixture-from-a-real-export).
