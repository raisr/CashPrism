namespace CashPrism.Application.Imports;

/// <summary>
/// What an import did. Every outcome a person can cause by picking the wrong file
/// is reported here rather than thrown.
/// </summary>
/// <param name="Outcome">How the import ended.</param>
/// <param name="ImportRunId">
/// The run that was created, or — for <see cref="ImportOutcome.AlreadyImported"/> —
/// the earlier run that already holds this file. <see cref="Guid.Empty"/> on failure.
/// </param>
/// <param name="RowsRead">How many data rows the file carried.</param>
/// <param name="BookingsInserted">How many bookings were stored for the first time.</param>
/// <param name="BookingsUpdated">How many stored bookings were replaced by a later state.</param>
/// <param name="BookingsUnchanged">
/// How many rows left the stored booking as it was — either because the row said
/// the same thing, or because the file turned out to be older than what is stored.
/// </param>
/// <param name="UnknownColumns">Header names this version of CashPrism does not know.</param>
/// <param name="Errors">What is wrong with the file. Empty unless the import failed.</param>
public sealed record ImportResult(
    ImportOutcome Outcome,
    Guid ImportRunId,
    int RowsRead,
    int BookingsInserted,
    int BookingsUpdated,
    int BookingsUnchanged,
    IReadOnlyList<string> UnknownColumns,
    IReadOnlyList<ImportError> Errors)
{
    /// <summary>Whether the file was accepted, whether or not it changed anything.</summary>
    public bool IsSuccess => Outcome is not ImportOutcome.Failed;

    /// <summary>A result for a file that was read and applied.</summary>
    /// <param name="importRunId">The run that was created.</param>
    /// <param name="rowsRead">How many data rows the file carried.</param>
    /// <param name="bookingsInserted">How many bookings were new.</param>
    /// <param name="bookingsUpdated">How many were replaced.</param>
    /// <param name="bookingsUnchanged">How many were left as they were.</param>
    /// <param name="unknownColumns">Header names that are not known columns.</param>
    public static ImportResult Imported(
        Guid importRunId,
        int rowsRead,
        int bookingsInserted,
        int bookingsUpdated,
        int bookingsUnchanged,
        IReadOnlyList<string> unknownColumns)
    {
        ArgumentNullException.ThrowIfNull(unknownColumns);

        return new ImportResult(
            ImportOutcome.Imported,
            importRunId,
            rowsRead,
            bookingsInserted,
            bookingsUpdated,
            bookingsUnchanged,
            unknownColumns,
            Errors: []);
    }

    /// <summary>A result for a file an earlier run already imported.</summary>
    /// <param name="earlierRunId">The run that holds this file.</param>
    public static ImportResult AlreadyImported(Guid earlierRunId)
        => new(
            ImportOutcome.AlreadyImported,
            earlierRunId,
            RowsRead: 0,
            BookingsInserted: 0,
            BookingsUpdated: 0,
            BookingsUnchanged: 0,
            UnknownColumns: [],
            Errors: []);

    /// <summary>A result for a file that could not be read.</summary>
    /// <param name="errors">What is wrong with it. At least one entry.</param>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static ImportResult Failed(IReadOnlyList<ImportError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("A failed import has to say what is wrong.", nameof(errors));
        }

        return new ImportResult(
            ImportOutcome.Failed,
            ImportRunId: Guid.Empty,
            RowsRead: 0,
            BookingsInserted: 0,
            BookingsUpdated: 0,
            BookingsUnchanged: 0,
            UnknownColumns: [],
            errors);
    }
}
