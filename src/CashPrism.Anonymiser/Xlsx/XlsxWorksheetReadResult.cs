namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// The outcome of reading and validating one input file's worksheet: either
/// everything a run needs to fold this file into the shared dictionaries and
/// later rewrite it, or the one reason it could not be read.
/// </summary>
/// <param name="WorksheetEntryName">The zip entry name of the worksheet part. <see langword="null"/> on failure.</param>
/// <param name="WorksheetXml">The worksheet's raw XML, unmodified. <see langword="null"/> on failure.</param>
/// <param name="ColumnLetters">This file's own column name to spreadsheet letter mapping. <see langword="null"/> on failure.</param>
/// <param name="ValuesByColumn">
/// The raw, still-escaped text of every non-empty data-row cell in one of the
/// replaced columns, keyed by <see cref="Infrastructure.Finanzguru.FinanzguruColumns"/>
/// name. <see langword="null"/> on failure.
/// </param>
/// <param name="DataRowCount">The rows the input file carried below its header. Zero on failure.</param>
/// <param name="RetainedRowCount">
/// The rows kept after applying <c>--max-rows</c> — equal to
/// <paramref name="DataRowCount"/> when no limit was given or it exceeded the
/// file. <see cref="WorksheetXml"/> already reflects this count. Zero on
/// failure.
/// </param>
/// <param name="ErrorMessage">The one line to print on stderr. <see langword="null"/> on success.</param>
public sealed record XlsxWorksheetReadResult(
    string? WorksheetEntryName,
    string? WorksheetXml,
    IReadOnlyDictionary<string, string>? ColumnLetters,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? ValuesByColumn,
    int DataRowCount,
    int RetainedRowCount,
    string? ErrorMessage)
{
    /// <summary>Whether the file was read successfully.</summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>Wraps a successfully read worksheet.</summary>
    public static XlsxWorksheetReadResult Success(
        string worksheetEntryName,
        string worksheetXml,
        IReadOnlyDictionary<string, string> columnLetters,
        IReadOnlyDictionary<string, IReadOnlyList<string>> valuesByColumn,
        int dataRowCount,
        int retainedRowCount)
        => new(worksheetEntryName, worksheetXml, columnLetters, valuesByColumn, dataRowCount, retainedRowCount, null);

    /// <summary>Wraps the one-line reason reading failed.</summary>
    public static XlsxWorksheetReadResult Failure(string errorMessage) => new(null, null, null, null, 0, 0, errorMessage);
}
