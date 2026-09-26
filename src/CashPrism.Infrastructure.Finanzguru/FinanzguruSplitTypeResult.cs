using CashPrism.Domain.Bookings;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The outcome of reading the <c>Split-Typ</c> column: either the role, or why
/// the cell could not be read.
/// </summary>
/// <param name="Value">The role the row plays, or <c>null</c> on failure.</param>
/// <param name="Error">What is wrong with the cell, or <c>null</c> on success.</param>
public sealed record FinanzguruSplitTypeResult(SplitRole? Value, string? Error)
{
    /// <summary>Whether the cell could be read.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>A successful result carrying <paramref name="value"/>.</summary>
    /// <param name="value">The role that was read.</param>
    public static FinanzguruSplitTypeResult Success(SplitRole value) => new(value, Error: null);

    /// <summary>A failed result carrying <paramref name="error"/>.</summary>
    /// <param name="error">What is wrong with the cell.</param>
    /// <exception cref="ArgumentException"><paramref name="error"/> is missing.</exception>
    public static FinanzguruSplitTypeResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new FinanzguruSplitTypeResult(Value: null, error);
    }
}
