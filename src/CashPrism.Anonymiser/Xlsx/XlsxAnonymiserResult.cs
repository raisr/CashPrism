namespace CashPrism.Anonymiser.Xlsx;

/// <summary>One successfully anonymised input file.</summary>
/// <param name="InputPath">The file that was read.</param>
/// <param name="OutputPath">The anonymised copy that was written.</param>
/// <param name="RowsRead">Data rows read from the input.</param>
/// <param name="RowsWritten">
/// Data rows carried into the output. Equal to <see cref="RowsRead"/> unless
/// <c>--max-rows</c> kept only a prefix of them.
/// </param>
public sealed record XlsxAnonymiserResult(string InputPath, string OutputPath, int RowsRead, int RowsWritten);
