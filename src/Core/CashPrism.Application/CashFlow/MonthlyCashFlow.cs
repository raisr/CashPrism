namespace CashPrism.Application.CashFlow;

/// <summary>Income and spending of one calendar month, in whole cents.</summary>
/// <param name="Month">The first day of the month.</param>
/// <param name="IncomeInCents">What came in.</param>
/// <param name="SpendingInCents">What went out, as a positive number.</param>
public sealed record MonthlyCashFlow(DateOnly Month, long IncomeInCents, long SpendingInCents)
{
    /// <summary>What was left: income minus spending, negative when more went out.</summary>
    public long LeftInCents => IncomeInCents - SpendingInCents;
}
