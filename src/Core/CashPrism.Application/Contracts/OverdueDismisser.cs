using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// Dismisses the overdue marker of one contract for the day its booking was
/// expected.
/// </summary>
public sealed class OverdueDismisser
{
    private readonly IDismissedOverdueStore store;

    /// <summary>Creates the use case.</summary>
    /// <param name="store">Keeps the dismissed markers.</param>
    public OverdueDismisser(IDismissedOverdueStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        this.store = store;
    }

    /// <summary>
    /// Dismisses the marker. It shows again once the contract is overdue on
    /// another expected date.
    /// </summary>
    /// <param name="contractId">Finanzguru's identifier of the contract.</param>
    /// <param name="expectedOn">The day the booking that did not come was expected.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <exception cref="ArgumentException">The contract identifier is missing.</exception>
    public Task DismissAsync(string contractId, DateOnly expectedOn, CancellationToken cancellationToken = default)
        => store.DismissAsync(new DismissedOverdue(contractId, expectedOn), cancellationToken);
}
