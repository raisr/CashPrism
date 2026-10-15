namespace CashPrism.Application.CashFlow;

/// <summary>
/// Sums the stored bookings counted by <see cref="Domain.CashFlow.CashFlowRule"/>.
/// The sums are taken where the bookings are, over whole cents, so no booking
/// is loaded to be added up.
/// </summary>
public interface ICashFlowReader
{
    /// <summary>
    /// The date the stored data is current to: the latest export date among
    /// the import runs, or the day of the latest import where no run carries
    /// an export date. <see langword="null"/> when nothing was imported.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// What each main category added up to per month, from
    /// <paramref name="firstMonth"/> to <paramref name="lastMonth"/> inclusive,
    /// ordered by month and category. A month and category without a counted
    /// booking is left out.
    /// </summary>
    /// <param name="firstMonth">The first day of the first month.</param>
    /// <param name="lastMonth">The first day of the last month.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<CategoryNet>> ReadCategoryNetsAsync(
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// What went out to each counterparty from <paramref name="firstMonth"/>
    /// to <paramref name="lastMonth"/> inclusive, largest first. Only bookings
    /// <see cref="Domain.CashFlow.CashFlowRule.CountsAsSpending"/> are added
    /// up, grouped by the counterparty exactly as the export carries it, so two
    /// spellings of one party stay two entries. A booking without a
    /// counterparty is left out: there is nobody to list it under.
    /// </summary>
    /// <param name="firstMonth">The first day of the first month.</param>
    /// <param name="lastMonth">The first day of the last month.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<CounterpartySpending>> ReadCounterpartySpendingAsync(
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default);
}
