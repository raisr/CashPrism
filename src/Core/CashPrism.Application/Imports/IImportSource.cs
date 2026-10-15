namespace CashPrism.Application.Imports;

/// <summary>
/// Turns an uploaded file into bookings. Implemented once per external format;
/// the use case neither knows nor cares which format it was handed.
/// </summary>
public interface IImportSource
{
    /// <summary>
    /// Reads <paramref name="stream"/> as an export.
    /// </summary>
    /// <param name="stream">The file's content, positioned at its start.</param>
    /// <param name="importRunId">
    /// The run the rows are being read for. It is passed in rather than assigned
    /// afterwards because a booking records the run its state came from, and a
    /// booking is built here — handing back a second set of field-for-field
    /// copies just to stamp them later would duplicate the whole model.
    /// </param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The export, or the reasons the file could not be read.</returns>
    Task<ImportSourceResult> ReadAsync(
        Stream stream,
        Guid importRunId,
        CancellationToken cancellationToken = default);
}
