namespace CashPrism.Application.Imports;

/// <summary>
/// What a source file said, read and ready to be stored.
/// </summary>
/// <param name="SheetName">
/// The name of the worksheet the rows came from, kept verbatim because the export
/// puts its date in there.
/// </param>
/// <param name="ExportedOn">
/// The date the export was taken, or <c>null</c> when the sheet name did not
/// carry one. It decides which of two states of a booking is the later one.
/// </param>
/// <param name="Bookings">The rows, in the order the file lists them.</param>
/// <param name="UnknownColumns">
/// Header names this version of CashPrism does not know. Reported rather than
/// rejected — an export that gained a column is still importable.
/// </param>
public sealed record ImportedData(
    string SheetName,
    DateOnly? ExportedOn,
    IReadOnlyList<ImportedBooking> Bookings,
    IReadOnlyList<string> UnknownColumns);
