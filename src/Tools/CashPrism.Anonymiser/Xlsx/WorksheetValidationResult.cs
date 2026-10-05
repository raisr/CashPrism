namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// The outcome of validating a worksheet part: either the number of data rows
/// it carries, or the one reason it is not something this tool can process.
/// </summary>
/// <param name="DataRowCount">The rows below the header. Zero on failure.</param>
/// <param name="ErrorMessage">The one line to print on stderr. <see langword="null"/> on success.</param>
public sealed record WorksheetValidationResult(int DataRowCount, string? ErrorMessage)
{
    /// <summary>Whether the worksheet is safe to copy through.</summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>Wraps a successfully validated row count.</summary>
    public static WorksheetValidationResult Success(int dataRowCount) => new(dataRowCount, null);

    /// <summary>Wraps the one-line reason validation failed.</summary>
    public static WorksheetValidationResult Failure(string errorMessage) => new(0, errorMessage);
}
