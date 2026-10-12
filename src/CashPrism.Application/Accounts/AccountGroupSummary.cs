using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// One account group as the accounts page shows it.
/// </summary>
/// <param name="Id">Identifies the group.</param>
/// <param name="Name">What the person called it.</param>
/// <param name="Accounts">Its shown accounts, in the order the person put them in.</param>
/// <param name="SumInCents">The sum of their balances, in whole cents; zero for an empty group.</param>
public sealed record AccountGroupSummary(
    Guid Id,
    string Name,
    IReadOnlyList<AccountBalance> Accounts,
    long SumInCents);
