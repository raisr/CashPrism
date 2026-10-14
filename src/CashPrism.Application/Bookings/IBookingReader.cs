using CashPrism.Application.CashFlow;
using CashPrism.Application.Paging;
using CashPrism.Domain.Bookings;

namespace CashPrism.Application.Bookings;

/// <summary>
/// Reads the stored bookings for display. Separate from <see cref="Imports.IImportStore"/>
/// because it answers a different question: the store writes one export at a
/// time, this hands out one screen at a time.
/// </summary>
/// <remarks>
/// A page at a time rather than the whole set: the measured export holds 6,327
/// bookings over six years, and every row a page asks for is rendered and sent
/// across the home network.
/// </remarks>
public interface IBookingReader
{
    /// <summary>
    /// Reads one page of the bookings <paramref name="request"/> filters for,
    /// ordered as it asks. The total counts the filtered bookings.
    /// </summary>
    /// <param name="request">Which slice, in which order.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<Page<Booking>> ReadPageAsync(BookingPageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// How many bookings are stored. Asked before a list is drawn at all, so an
    /// empty database can say so instead of showing an empty table.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums every booking <paramref name="filter"/> lists, not only one page of
    /// them, over whole cents.
    /// </summary>
    /// <param name="filter">Which bookings are listed.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<BookingSummary> ReadSummaryAsync(BookingFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// What came in and what went out per month, from
    /// <paramref name="firstMonth"/> to <paramref name="lastMonth"/> inclusive,
    /// over the bookings <paramref name="filter"/> lists, oldest first. Each
    /// booking is income or spending by its own sign, summed as
    /// <see cref="ReadSummaryAsync"/> sums them. A month without a summed
    /// booking is left out.
    /// </summary>
    /// <param name="filter">Which bookings are listed.</param>
    /// <param name="firstMonth">The first day of the first month.</param>
    /// <param name="lastMonth">The first day of the last month.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<MonthlyCashFlow>> ReadMonthlyFlowsAsync(
        BookingFilter filter,
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default);

    /// <summary>The accounts and categories the stored bookings carry, to filter by.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<BookingFilterOptions> ReadFilterOptionsAsync(CancellationToken cancellationToken = default);
}
