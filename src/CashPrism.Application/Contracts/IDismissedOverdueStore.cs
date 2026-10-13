using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// Keeps the overdue markers the person dismissed.
/// </summary>
public interface IDismissedOverdueStore
{
    /// <summary>
    /// Every stored dismissal, in no particular order.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<DismissedOverdue>> ReadAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores <paramref name="dismissal"/> in place of any earlier one of the
    /// same contract, which belongs to a date that is no longer expected. A
    /// dismissal already stored is left as it is.
    /// </summary>
    /// <param name="dismissal">The contract and the date whose marker is dismissed.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task DismissAsync(DismissedOverdue dismissal, CancellationToken cancellationToken = default);
}
