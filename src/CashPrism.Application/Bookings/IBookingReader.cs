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
    /// Reads one page of bookings, ordered as <paramref name="request"/> asks.
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
}
