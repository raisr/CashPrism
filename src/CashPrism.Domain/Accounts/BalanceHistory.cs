namespace CashPrism.Domain.Accounts;

/// <summary>
/// An account's balance at the end of each day it was asked for, as
/// <see cref="BalanceHistoryRule"/> walks it back.
/// </summary>
/// <param name="Holds">
/// Whether the walk agrees with the export at every day. When it does not,
/// there are no balances.
/// </param>
/// <param name="BalancesInCents">
/// One balance per day asked for, in the same order; <see langword="null"/> for
/// a day before the account's first booking.
/// </param>
public sealed record BalanceHistory(bool Holds, IReadOnlyList<long?> BalancesInCents)
{
    /// <summary>The history of an account whose walk disagrees with the export.</summary>
    public static BalanceHistory DoesNotHold { get; } = new(Holds: false, []);
}
