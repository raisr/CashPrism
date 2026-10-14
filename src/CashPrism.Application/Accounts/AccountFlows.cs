using CashPrism.Application.Bookings;
using CashPrism.Application.CashFlow;
using CashPrism.Domain.Bookings;

namespace CashPrism.Application.Accounts;

/// <summary>
/// The money that moved through one account: what came in and went out per
/// month, and what happened lately.
/// </summary>
/// <param name="Months">
/// <see cref="AccountFlowReader.MonthsShown"/> complete months, oldest first,
/// a month without bookings as zero.
/// </param>
/// <param name="Recent">The days counted as lately, up to the date the stored data is current to.</param>
/// <param name="RecentSummary">What came in and went out over <paramref name="Recent"/>.</param>
/// <param name="RecentBookings">
/// The latest bookings of <paramref name="Recent"/>, newest first, at most
/// <see cref="AccountFlowReader.RecentBookingsShown"/>.
/// </param>
public sealed record AccountFlows(
    IReadOnlyList<MonthlyCashFlow> Months,
    BookingPeriod Recent,
    BookingSummary RecentSummary,
    IReadOnlyList<Booking> RecentBookings);
