namespace CashPrism.Application.Bookings;

/// <summary>
/// One page of the booking list: which slice, in which order.
/// </summary>
/// <param name="Skip">How many bookings to pass over. Zero is the first page.</param>
/// <param name="Take">How many bookings the page holds.</param>
/// <param name="SortBy">The column to order by.</param>
/// <param name="Descending">Whether the order runs from the largest value down.</param>
public sealed record BookingPageRequest(
    int Skip,
    int Take,
    BookingSortColumn SortBy = BookingSortColumn.BookedOn,
    bool Descending = true)
{
    /// <summary>The largest page anything may ask for.</summary>
    /// <remarks>
    /// A page size arrives from a browser, and on Blazor Server every row it
    /// names is rendered and pushed down the circuit. The ceiling keeps a
    /// tampered-with request from turning the whole history into one render.
    /// </remarks>
    public const int MaxTake = 500;

    /// <summary>How many bookings to pass over. Zero is the first page.</summary>
    public int Skip { get; } = Skip >= 0
        ? Skip
        : throw new ArgumentOutOfRangeException(nameof(Skip), Skip, "A page cannot start before the first booking.");

    /// <summary>How many bookings the page holds.</summary>
    public int Take { get; } = Take is > 0 and <= MaxTake
        ? Take
        : throw new ArgumentOutOfRangeException(
            nameof(Take),
            Take,
            $"A page holds between one and {MaxTake} bookings.");

    /// <summary>The column to order by.</summary>
    public BookingSortColumn SortBy { get; } = Enum.IsDefined(SortBy)
        ? SortBy
        : throw new ArgumentOutOfRangeException(nameof(SortBy), SortBy, "Unknown sort column.");
}
