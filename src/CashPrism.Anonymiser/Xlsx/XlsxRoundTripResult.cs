namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// The outcome of round-tripping one input file: either the written output
/// with its row counts, or the one reason the run stopped.
/// </summary>
/// <param name="OutputPath">The written file. <see langword="null"/> on failure.</param>
/// <param name="RowsRead">Data rows read from the input. Zero on failure.</param>
/// <param name="RowsWritten">
/// Data rows carried into the output. Equal to <see cref="RowsRead"/> — this
/// ticket copies every row through unchanged, no anonymisation drops or
/// re-shapes any of them.
/// </param>
/// <param name="ErrorMessage">The one line to print on stderr. <see langword="null"/> on success.</param>
public sealed record XlsxRoundTripResult(string? OutputPath, int RowsRead, int RowsWritten, string? ErrorMessage)
{
    /// <summary>Whether the file was written.</summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>Wraps a successfully written output file.</summary>
    public static XlsxRoundTripResult Success(string outputPath, int rowsRead, int rowsWritten)
        => new(outputPath, rowsRead, rowsWritten, null);

    /// <summary>Wraps the one-line reason the run stopped.</summary>
    public static XlsxRoundTripResult Failure(string errorMessage) => new(null, 0, 0, errorMessage);
}
