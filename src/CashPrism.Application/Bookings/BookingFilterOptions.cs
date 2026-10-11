namespace CashPrism.Application.Bookings;

/// <summary>
/// The values the booking filter offers to pick from: only what the stored
/// bookings carry, so no choice can lead to a list that is empty by
/// construction.
/// </summary>
/// <param name="Accounts">The accounts, by name.</param>
/// <param name="Categories">The main categories, in ordinal order.</param>
public sealed record BookingFilterOptions(
    IReadOnlyList<BookingAccount> Accounts,
    IReadOnlyList<BookingCategory> Categories);
