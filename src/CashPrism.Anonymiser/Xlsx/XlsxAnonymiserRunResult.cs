namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// The outcome of one invocation of the tool, across every input file.
/// </summary>
/// <param name="FileResults">
/// Every file successfully anonymised before the run stopped. On success this
/// is every input file; on failure it is a strict prefix — the files already
/// written stay on disk, the run simply goes no further.
/// </param>
/// <param name="ErrorMessage">The one line to print on stderr. <see langword="null"/> on success.</param>
public sealed record XlsxAnonymiserRunResult(IReadOnlyList<XlsxAnonymiserResult> FileResults, string? ErrorMessage)
{
    /// <summary>Whether every input file was anonymised.</summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>Wraps a fully successful run.</summary>
    public static XlsxAnonymiserRunResult Success(IReadOnlyList<XlsxAnonymiserResult> fileResults) => new(fileResults, null);

    /// <summary>Wraps the one-line reason the run stopped, along with whatever succeeded before it.</summary>
    public static XlsxAnonymiserRunResult Failure(IReadOnlyList<XlsxAnonymiserResult> fileResults, string errorMessage)
        => new(fileResults, errorMessage);

    /// <summary>Wraps the one-line reason the run stopped before anything was written.</summary>
    public static XlsxAnonymiserRunResult Failure(string errorMessage) => new([], errorMessage);
}
