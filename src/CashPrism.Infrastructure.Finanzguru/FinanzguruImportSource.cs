using CashPrism.Application.Imports;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The FinanzGuru side of an import: reads the export with
/// <see cref="FinanzguruExportReader"/> and hands the use case bookings it can
/// store, so that nothing above this layer has to know a German column name.
/// </summary>
public sealed class FinanzguruImportSource : IImportSource
{
    private readonly FinanzguruExportReader reader = new();

    /// <inheritdoc />
    /// <remarks>
    /// Reading the workbook is synchronous work — ClosedXML has no asynchronous
    /// API and the file is already in memory by the time it gets here — so the
    /// method is asynchronous only because the interface is. Wrapping it in
    /// <c>Task.Run</c> to look busy would buy a thread and nothing else.
    /// </remarks>
    public Task<ImportSourceResult> ReadAsync(
        Stream stream,
        Guid importRunId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        cancellationToken.ThrowIfCancellationRequested();

        var read = reader.Read(stream);

        if (!read.IsSuccess)
        {
            return Task.FromResult(new ImportSourceResult(Data: null, read.Errors));
        }

        var export = read.Export!;
        var bookings = new List<ImportedBooking>(export.Rows.Count);
        var errors = new List<string>();

        foreach (var row in export.Rows)
        {
            var booking = FinanzguruBooking.Create(row, importRunId);

            if (!booking.IsSuccess)
            {
                errors.AddRange(booking.Errors);

                continue;
            }

            bookings.Add(new ImportedBooking(
                booking.Value!,
                FinanzguruRawRow.ToJson(row.Values),
                row.RowNumber));
        }

        if (errors.Count > 0)
        {
            return Task.FromResult(new ImportSourceResult(Data: null, errors));
        }

        return Task.FromResult(ImportSourceResult.Success(new ImportedData(
            export.SheetName,
            export.ExportedOn,
            bookings,
            export.UnknownColumns)));
    }
}
