using CashPrism.Domain.Bookings;

namespace CashPrism.Application.Bookings;

/// <summary>
/// One page of bookings, and how many there are in total.
/// </summary>
/// <param name="Bookings">The bookings on this page, in the requested order.</param>
/// <param name="TotalCount">
/// How many bookings the database holds in total, not how many this page
/// carries. It is what a pager counts its pages from.
/// </param>
public sealed record BookingPage(IReadOnlyList<Booking> Bookings, int TotalCount);
