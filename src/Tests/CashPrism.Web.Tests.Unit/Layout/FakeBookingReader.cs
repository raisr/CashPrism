using CashPrism.Application.Bookings;
using CashPrism.Application.Paging;
using CashPrism.Domain.Bookings;

namespace CashPrism.Web.Tests.Unit.Layout;

/// <summary>
/// Stands in for the stored bookings. Only the count is read by the drawer, so
/// only the count is backed by anything.
/// </summary>
public sealed class FakeBookingReader : IBookingReader
{
    /// <summary>How many bookings the fake claims are stored. Settable, so a test can import some.</summary>
    public int Count { get; set; }

    public Task<Page<Booking>> ReadPageAsync(BookingPageRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("The drawer never reads a page of bookings.");

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Count);
}
