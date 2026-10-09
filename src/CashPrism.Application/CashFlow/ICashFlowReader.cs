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
}
