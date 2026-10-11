namespace CashPrism.Application.Bookings;

/// <summary>
/// A bound a search puts on the amount of a booking, compared without the
/// booking's sign: <c>&gt;200</c> finds spending and income above 200 € alike.
/// </summary>
/// <param name="Comparison">How the booking's amount compares with the bound.</param>
/// <param name="AmountInCents">The bound in whole cents, never negative.</param>
public sealed record AmountBound(AmountComparison Comparison, long AmountInCents);
