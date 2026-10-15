namespace CashPrism.Application.Bookings;

/// <summary>
/// A period the booking list offers ready-made. Each one is a run of whole
/// calendar months, counted from the date the stored data is current to — see
/// <see cref="BookingPeriod.For"/>.
/// </summary>
public enum BookingPeriodPreset
{
    /// <summary>The month the stored data is current to.</summary>
    ThisMonth = 1,

    /// <summary>The month before that.</summary>
    LastMonth = 2,

    /// <summary>The month the stored data is current to and the eleven before it.</summary>
    Last12Months = 3,

    /// <summary>The year the stored data is current to.</summary>
    ThisYear = 4,

    /// <summary>The year before that.</summary>
    LastYear = 5,
}
