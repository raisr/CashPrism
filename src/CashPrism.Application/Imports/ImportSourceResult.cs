namespace CashPrism.Application.Imports;

/// <summary>
/// The outcome of reading an export: either the export, or the reasons the file
/// could not be read. Being handed a file that is not an export at all is an
/// expected outcome of letting a person pick one, so it is a result rather than
/// an exception.
/// </summary>
/// <param name="Data">What the file said, or <c>null</c> on failure.</param>
/// <param name="Errors">
/// What is wrong with the file, one entry per problem. Empty on success.
/// </param>
public sealed record ImportSourceResult(ImportedData? Data, IReadOnlyList<string> Errors)
{
    /// <summary>Whether the file could be read.</summary>
    public bool IsSuccess => Data is not null;

    /// <summary>A successful result carrying <paramref name="data"/>.</summary>
    /// <param name="data">What the file said.</param>
    public static ImportSourceResult Success(ImportedData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new ImportSourceResult(data, Errors: []);
    }

    /// <summary>A failed result carrying <paramref name="errors"/>.</summary>
    /// <param name="errors">What is wrong with the file. At least one entry.</param>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static ImportSourceResult Failure(params string[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Length == 0)
        {
            throw new ArgumentException("A failed read has to say what is wrong.", nameof(errors));
        }

        return new ImportSourceResult(Data: null, errors);
    }
}
