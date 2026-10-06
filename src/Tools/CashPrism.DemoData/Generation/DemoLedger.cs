using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// Turns the planned bookings into rows: works out each account's balance in
/// date order and the columns that follow from the date or the amount alone,
/// then orders the rows newest first.
/// </summary>
public static class DemoLedger
{
    /// <summary>The word <c>Analyse-Betrag</c> carries for income.</summary>
    public const string Income = "Einnahmen";

    /// <summary>The word <c>Analyse-Betrag</c> carries for spending.</summary>
    public const string Spending = "Ausgaben";

    /// <summary>The rows for <paramref name="bookings"/>, newest first.</summary>
    /// <remarks>
    /// The two parts of a split booking repeat the original's money rather than
    /// adding to it, so they leave the balance where the original put it.
    /// </remarks>
    public static IReadOnlyList<DemoRow> Rows(IReadOnlyList<DemoBooking> bookings)
    {
        ArgumentNullException.ThrowIfNull(bookings);

        var balances = DemoHousehold.Accounts.ToDictionary(account => account, account => account.OpeningBalanceInCents);
        var rows = new List<(DemoRow Row, int Order)>(bookings.Count);

        var chronological = bookings
            .Select((booking, order) => (Booking: booking, Order: order))
            .OrderBy(entry => entry.Booking.Date)
            .ThenBy(entry => entry.Order);

        foreach (var (booking, order) in chronological)
        {
            if (booking.SplitType is not (FinanzguruSplitType.Part or FinanzguruSplitType.Remainder))
            {
                balances[booking.Account] += booking.AmountInCents;
            }

            rows.Add((Row(booking, balances[booking.Account]), order));
        }

        return rows
            .OrderByDescending(entry => entry.Row.Booking.Date)
            .ThenByDescending(entry => entry.Order)
            .Select(entry => entry.Row)
            .ToList();
    }

    private static DemoRow Row(DemoBooking booking, long balanceInCents)
    {
        var date = DateOnly.FromDateTime(booking.Date);

        return new DemoRow(
            booking,
            balanceInCents,
            booking.AmountInCents > 0 ? Income : Spending,
            FinanzguruPeriodLabels.Week(date),
            FinanzguruPeriodLabels.Month(date),
            FinanzguruPeriodLabels.Quarter(date),
            date.Year);
    }
}
