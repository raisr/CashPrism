using CashPrism.Domain.Bookings;

namespace CashPrism.Domain.Accounts;

/// <summary>
/// How an account's balance at the end of an earlier day follows from the
/// balance taken at import: every booking dated after that day, up to the day
/// the balance was taken, is subtracted again.
/// </summary>
/// <remarks>
/// <para>
/// Transfers count, because they move the account's money. Of a split booking
/// only the parts count: they add up to the original. A booking dated after
/// the day the balance was taken does not count, because the balance does not
/// contain it yet.
/// </para>
/// <para>
/// The walk is checked against the export at every day it is asked for: the
/// walked balance has to be one of the <c>Kontostand</c> values the account's
/// rows carry on its last booking day up to that day. Where one day fails, the
/// account has no history at all — a line that is wrong somewhere cannot be
/// trusted anywhere. A day before the account's first booking has no balance:
/// the account may have held money before Finanzguru recorded it, and nothing
/// says how much. The measurement this rests on is in
/// <c>docs/finanzguru-export.md</c>.
/// </para>
/// </remarks>
public static class BalanceHistoryRule
{
    /// <summary>
    /// The account's balance at the end of each of <paramref name="days"/>.
    /// </summary>
    /// <param name="balanceInCents">The balance taken at import.</param>
    /// <param name="balanceOn">The day the balance was taken on: the export date.</param>
    /// <param name="movements">Every booking of the account.</param>
    /// <param name="days">The days to give the balance for.</param>
    public static BalanceHistory For(
        long balanceInCents,
        DateOnly balanceOn,
        IReadOnlyCollection<BalanceMovement> movements,
        IReadOnlyList<DateOnly> days)
    {
        ArgumentNullException.ThrowIfNull(movements);
        ArgumentNullException.ThrowIfNull(days);

        var balances = new List<long?>(days.Count);

        foreach (var day in days)
        {
            var upToDay = movements.Where(movement => movement.BookedOn <= day).ToList();

            if (upToDay.Count == 0)
            {
                balances.Add(null);
                continue;
            }

            var walked = balanceInCents - movements
                .Where(movement => movement.BookedOn > day && movement.BookedOn <= balanceOn)
                .Where(movement => movement.SplitRole != SplitRole.Original)
                .Sum(movement => movement.AmountInCents);

            var lastDay = upToDay.Max(movement => movement.BookedOn);

            if (!upToDay.Any(movement => movement.BookedOn == lastDay && movement.ReportedBalanceInCents == walked))
            {
                return BalanceHistory.DoesNotHold;
            }

            balances.Add(walked);
        }

        return new BalanceHistory(Holds: true, balances);
    }
}
