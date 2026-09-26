namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// A FinanzGuru export that was read successfully: when it was taken, what it
/// said, and which columns this version of CashPrism does not know about.
/// </summary>
/// <param name="ExportedOn">
/// The date the export was taken, read from the worksheet name. It decides which
/// version of a booking wins on a re-import.
/// </param>
/// <param name="Rows">The data rows, in worksheet order.</param>
/// <param name="UnknownColumns">
/// The header names that are not known columns, each listed once. Reported
/// rather than rejected: an export that gained a column is still readable, and
/// what the column means is the caller's decision, not this reader's.
/// </param>
public sealed record FinanzguruExport(
    DateOnly ExportedOn,
    IReadOnlyList<FinanzguruExportRow> Rows,
    IReadOnlyList<string> UnknownColumns);
