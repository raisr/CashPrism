namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The outcome of reading a FinanzGuru export: either the export, or the reasons
/// the file could not be read. Being handed a file that is not a FinanzGuru
/// export is an expected outcome of letting a person pick one, so it is reported
/// as a result rather than thrown.
/// </summary>
/// <param name="Export">The export that was read, or <c>null</c> on failure.</param>
/// <param name="Errors">
/// What is wrong with the file, one entry per problem, each naming what it found
/// and where. Empty on success.
/// </param>
public sealed record FinanzguruExportReadResult(FinanzguruExport? Export, IReadOnlyList<string> Errors)
{
    /// <summary>Whether the file could be read.</summary>
    public bool IsSuccess => Export is not null;

    /// <summary>A successful result carrying <paramref name="export"/>.</summary>
    /// <param name="export">The export that was read.</param>
    public static FinanzguruExportReadResult Success(FinanzguruExport export)
    {
        ArgumentNullException.ThrowIfNull(export);

        return new FinanzguruExportReadResult(export, Errors: []);
    }

    /// <summary>A failed result carrying <paramref name="errors"/>.</summary>
    /// <param name="errors">What is wrong with the file. At least one entry.</param>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static FinanzguruExportReadResult Failure(params string[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Length == 0)
        {
            throw new ArgumentException("A failed read has to say what is wrong.", nameof(errors));
        }

        return new FinanzguruExportReadResult(Export: null, errors);
    }
}
