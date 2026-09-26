namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The outcome of reading a money cell: either the amount in whole cents, or the
/// reason the cell could not be read. A foreign export carrying a number we do
/// not understand is an expected outcome of reading it, so it is reported as a
/// result rather than thrown.
/// </summary>
/// <param name="AmountInCents">
/// The amount in whole cents, or <c>null</c> on failure.
/// </param>
/// <param name="Error">
/// What is wrong with the cell, naming the column and the row so the file can be
/// looked at. <c>null</c> on success.
/// </param>
public sealed record FinanzguruAmountResult(long? AmountInCents, string? Error)
{
    /// <summary>Whether the cell could be read.</summary>
    public bool IsSuccess => AmountInCents.HasValue;

    /// <summary>A successful result carrying <paramref name="amountInCents"/>.</summary>
    /// <param name="amountInCents">The amount the cell carried, in whole cents.</param>
    public static FinanzguruAmountResult Success(long amountInCents) => new(amountInCents, Error: null);

    /// <summary>A failed result carrying <paramref name="error"/>.</summary>
    /// <param name="error">What is wrong with the cell.</param>
    public static FinanzguruAmountResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new FinanzguruAmountResult(AmountInCents: null, error);
    }
}
