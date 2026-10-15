namespace CashPrism.Application.Accounts;

/// <summary>The balance of one account at the end of a day.</summary>
/// <param name="Day">The day.</param>
/// <param name="BalanceInCents">The balance at its end, in whole cents.</param>
public sealed record DatedBalance(DateOnly Day, long BalanceInCents);
