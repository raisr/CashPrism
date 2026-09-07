namespace CashPrism.Anonymiser.Xlsx;

/// <summary>The outcome of writing one anonymised output file.</summary>
/// <param name="ErrorMessage">The one line to print on stderr. <see langword="null"/> on success.</param>
public sealed record XlsxAnonymiserWriteResult(string? ErrorMessage)
{
    /// <summary>Whether the file was written.</summary>
    public bool IsSuccess => ErrorMessage is null;

    /// <summary>Wraps a successful write.</summary>
    public static XlsxAnonymiserWriteResult Success() => new((string?)null);

    /// <summary>Wraps the one-line reason writing failed.</summary>
    public static XlsxAnonymiserWriteResult Failure(string errorMessage) => new(errorMessage);
}
