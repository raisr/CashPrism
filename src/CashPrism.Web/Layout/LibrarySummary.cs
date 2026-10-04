namespace CashPrism.Web.Layout;

/// <summary>
/// What the drawer says about the stored data: how much there is, and how fresh
/// it is.
/// </summary>
/// <param name="BookingCount">How many bookings are stored.</param>
/// <param name="LastImportedAt">
/// When the latest import run happened, or <c>null</c> when nothing was ever
/// imported.
/// </param>
public sealed record LibrarySummary(int BookingCount, DateTimeOffset? LastImportedAt);
