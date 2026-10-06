namespace CashPrism.DemoData.CommandLine;

/// <summary>
/// The outcome of parsing the command line: either the resolved
/// <see cref="DemoDataOptions"/>, or one human-readable reason the arguments do
/// not add up to a runnable command.
/// </summary>
/// <param name="Options">The parsed options. <see langword="null"/> on failure.</param>
/// <param name="ErrorMessage">
/// The one line to print on stderr. <see langword="null"/> on success.
/// </param>
public sealed record DemoDataCommandLineResult(DemoDataOptions? Options, string? ErrorMessage)
{
    /// <summary>Whether the arguments resolved to a runnable <see cref="Options"/>.</summary>
    public bool IsSuccess => Options is not null;

    /// <summary>Wraps a successfully parsed set of options.</summary>
    public static DemoDataCommandLineResult Success(DemoDataOptions options) => new(options, null);

    /// <summary>Wraps the one-line reason parsing failed.</summary>
    public static DemoDataCommandLineResult Failure(string errorMessage) => new(null, errorMessage);
}
