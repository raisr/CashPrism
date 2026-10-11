namespace CashPrism.Application.Accounts;

/// <summary>
/// Reads the accounts the way the accounts page and the overview show them:
/// sorted by balance and totalled.
/// </summary>
public sealed class AccountOverviewReader
{
    private readonly IAccountBalanceReader reader;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="reader">Reads the stored balances.</param>
    public AccountOverviewReader(IAccountBalanceReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        this.reader = reader;
    }

    /// <summary>
    /// The accounts of the latest export, or <c>null</c> when no export has
    /// reported a balance — nothing imported yet, or only before CashPrism
    /// took balances at import.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<AccountOverview?> ReadAsync(CancellationToken cancellationToken = default)
    {
        var balances = await reader.ReadLatestAsync(cancellationToken);

        if (balances.Count == 0)
        {
            return null;
        }

        // The name breaks a tie, so two accounts with the same balance — two
        // empty ones, typically — keep one order from one visit to the next.
        var accounts = balances
            .OrderByDescending(balance => balance.BalanceInCents)
            .ThenBy(balance => balance.AccountName, StringComparer.CurrentCulture)
            .ThenBy(balance => balance.AccountReference, StringComparer.Ordinal)
            .ToList();

        return new AccountOverview(accounts, accounts.Sum(balance => balance.BalanceInCents));
    }
}
