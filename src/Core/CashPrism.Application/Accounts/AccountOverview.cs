using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Every account the latest export carries, with what they hold together.
/// </summary>
/// <param name="Accounts">The accounts, the highest balance first.</param>
/// <param name="TotalInCents">
/// The sum of all balances, in whole cents. A loan account counts with its
/// negative balance, so this is what the accounts hold net — the figure
/// Finanzguru shows over its own list of accounts.
/// </param>
public sealed record AccountOverview(IReadOnlyList<AccountBalance> Accounts, long TotalInCents);
