namespace CashPrism.Application.Bookings;

/// <summary>
/// Which way the money of a booking went, read from the sign of its amount.
/// </summary>
public enum BookingDirection
{
    /// <summary>Money came in: the amount is positive.</summary>
    Income = 1,

    /// <summary>Money went out: the amount is negative.</summary>
    Spending = 2,
}
