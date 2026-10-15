using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// The order accounts are listed in where nobody has placed them: the highest
/// balance first.
/// </summary>
internal static class AccountOrder
{
    /// <summary>Sorts <paramref name="balances"/> by balance, the highest first.</summary>
    /// <param name="balances">The balances to sort.</param>
    public static List<AccountBalance> ByBalance(IEnumerable<AccountBalance> balances)
    {
        // The name breaks a tie, so two accounts with the same balance — two
        // empty ones, typically — keep one order from one visit to the next.
        return [.. balances
            .OrderByDescending(balance => balance.BalanceInCents)
            .ThenBy(balance => balance.AccountName, StringComparer.CurrentCulture)
            .ThenBy(balance => balance.AccountReference, StringComparer.Ordinal)];
    }
}
