namespace CashPrism.Application.Bookings;

/// <summary>
/// What the booking list is narrowed to. Every criterion is optional and takes
/// a single value; the ones that are set must all hold for a booking to be
/// listed.
/// </summary>
/// <param name="AccountReference">The account, by the reference the export carries — an IBAN or a provider's handle.</param>
/// <param name="Category">Finanzguru's main category, as the analysis groups by it.</param>
/// <param name="SubCategory">A sub-category of <paramref name="Category"/>. Only together with it.</param>
/// <param name="Period">The days the booking was posted on.</param>
/// <param name="Counterparty">The other party, exactly as the export carries it.</param>
/// <param name="Direction">Whether money came in or went out.</param>
/// <param name="HideTransfers">Whether transfers between two of the owner's own accounts are left out.</param>
/// <param name="Search">Words and amount bounds a person searched for.</param>
public sealed record BookingFilter(
    string? AccountReference = null,
    string? Category = null,
    string? SubCategory = null,
    BookingPeriod? Period = null,
    string? Counterparty = null,
    BookingDirection? Direction = null,
    bool HideTransfers = false,
    BookingSearch? Search = null)
{
    /// <summary>The filter that lists every booking.</summary>
    public static BookingFilter None { get; } = new();

    /// <summary>
    /// A sub-category of <see cref="Category"/>. It is picked from within its
    /// main category and narrows that one further, so it never stands alone.
    /// </summary>
    public string? SubCategory { get; } = SubCategory is null || Category is not null
        ? SubCategory
        : throw new ArgumentException("A sub-category needs the main category it belongs to.", nameof(SubCategory));

    /// <summary>Whether money came in or went out.</summary>
    public BookingDirection? Direction { get; } = Direction is not { } direction || Enum.IsDefined(direction)
        ? Direction
        : throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Unknown direction.");
}
