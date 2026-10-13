namespace CashPrism.Domain.Contracts;

/// <summary>
/// An overdue marker the person dismissed: one contract's booking expected on
/// one day. The export knows nothing about it, so this is CashPrism's own
/// state, not a projection.
/// </summary>
/// <remarks>
/// The expected date is part of what is dismissed, not only the contract:
/// once the contract books again, or its next date is overdue in turn, the
/// marker belongs to a different date and shows again.
/// </remarks>
public sealed class DismissedOverdue
{
    /// <summary>
    /// Creates the dismissal.
    /// </summary>
    /// <param name="contractId">Finanzguru's identifier of the contract.</param>
    /// <param name="expectedOn">The day the booking that did not come was expected.</param>
    /// <exception cref="ArgumentException">The contract identifier is missing.</exception>
    public DismissedOverdue(string contractId, DateOnly expectedOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractId);

        ContractId = contractId;
        ExpectedOn = expectedOn;
    }

    /// <summary>Finanzguru's identifier of the contract.</summary>
    public string ContractId { get; }

    /// <summary>The day the booking that did not come was expected.</summary>
    public DateOnly ExpectedOn { get; }
}
