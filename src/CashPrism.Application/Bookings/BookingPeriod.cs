namespace CashPrism.Application.Bookings;

/// <summary>
/// A run of days the booking list is narrowed to, both ends included. Always
/// two absolute dates, also when it was picked as a preset, so a link to a
/// filtered list shows the same bookings whenever it is opened.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
public sealed record BookingPeriod(DateOnly From, DateOnly To)
{
    /// <summary>The last day.</summary>
    public DateOnly To { get; } = To >= From
        ? To
        : throw new ArgumentOutOfRangeException(nameof(To), To, "A period cannot end before it starts.");

    /// <summary>
    /// The period <paramref name="preset"/> stands for. Counted from
    /// <paramref name="currentTo"/>, the date the stored data is current to,
    /// rather than from today, as on the overview: an export a few weeks old
    /// would otherwise make "this month" an empty one.
    /// </summary>
    /// <param name="preset">The ready-made period.</param>
    /// <param name="currentTo">The date the stored data is current to.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="preset"/> is not a known preset.</exception>
    public static BookingPeriod For(BookingPeriodPreset preset, DateOnly currentTo)
    {
        var month = new DateOnly(currentTo.Year, currentTo.Month, 1);
        var year = new DateOnly(currentTo.Year, 1, 1);

        return preset switch
        {
            BookingPeriodPreset.ThisMonth => Months(month, 1),
            BookingPeriodPreset.LastMonth => Months(month.AddMonths(-1), 1),
            BookingPeriodPreset.Last12Months => Months(month.AddMonths(-11), 12),
            BookingPeriodPreset.ThisYear => Months(year, 12),
            BookingPeriodPreset.LastYear => Months(year.AddYears(-1), 12),
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown period preset."),
        };
    }

    private static BookingPeriod Months(DateOnly first, int count)
        => new(first, first.AddMonths(count).AddDays(-1));
}
