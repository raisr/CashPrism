namespace CashPrism.Domain.CashFlow;

/// <summary>
/// What one booking contributes to income and spending, as
/// <see cref="CashFlowRule"/> decides it. Both amounts are whole cents and
/// never negative: at most one of them is not zero.
/// </summary>
/// <remarks>
/// A class with settable properties rather than a record with a constructor,
/// because the rule is handed to the database as an expression: a query
/// provider translates a member initialisation reliably and a constructor
/// call does not.
/// </remarks>
public sealed class CashFlowEntry
{
    /// <summary>The date the booking was posted.</summary>
    public DateTime BookedOn { get; init; }

    /// <summary>The booking's main category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>What the booking brought in, in whole cents.</summary>
    public long IncomeInCents { get; init; }

    /// <summary>What the booking took out, in whole cents, as a positive number.</summary>
    public long SpendingInCents { get; init; }
}
