namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The outcome of reading a yes/no cell: either the value, or the reason the cell
/// could not be read. A foreign export carrying something we do not understand is
/// an expected outcome of reading it, so it is reported as a result rather than
/// thrown.
/// </summary>
/// <param name="Value">The value the cell carried, or <c>null</c> on failure.</param>
/// <param name="Error">
/// What is wrong with the cell, naming the column and the row so the file can be
/// looked at. <c>null</c> on success.
/// </param>
public sealed record FinanzguruFlagResult(bool? Value, string? Error)
{
    /// <summary>Whether the cell could be read.</summary>
    public bool IsSuccess => Value.HasValue;

    /// <summary>A successful result carrying <paramref name="value"/>.</summary>
    /// <param name="value">The value the cell carried.</param>
    public static FinanzguruFlagResult Success(bool value) => new(value, Error: null);

    /// <summary>A failed result carrying <paramref name="error"/>.</summary>
    /// <param name="error">What is wrong with the cell.</param>
    public static FinanzguruFlagResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new FinanzguruFlagResult(Value: null, Error: error);
    }
}
