using CashPrism.Application.Paging;

namespace CashPrism.Application.Bookings;

/// <summary>
/// One page of the booking list: which bookings, which slice, in which order.
/// The slice and its guards come from <see cref="PageRequest"/>; what this adds
/// is the filter and the ordering, which only a list of bookings has.
/// </summary>
/// <param name="Skip">How many bookings to pass over. Zero is the first page.</param>
/// <param name="Take">How many bookings the page holds.</param>
/// <param name="SortBy">The column to order by.</param>
/// <param name="Descending">Whether the order runs from the largest value down.</param>
/// <param name="Filter">Which bookings are listed. Every one when left out.</param>
public sealed record BookingPageRequest(
    int Skip,
    int Take,
    BookingSortColumn SortBy = BookingSortColumn.BookedOn,
    bool Descending = true,
    BookingFilter? Filter = null)
    : PageRequest(Skip, Take)
{
    /// <summary>Which bookings are listed.</summary>
    public BookingFilter Filter { get; } = Filter ?? BookingFilter.None;

    /// <summary>The column to order by.</summary>
    public BookingSortColumn SortBy { get; } = Enum.IsDefined(SortBy)
        ? SortBy
        : throw new ArgumentOutOfRangeException(nameof(SortBy), SortBy, "Unknown sort column.");
}
