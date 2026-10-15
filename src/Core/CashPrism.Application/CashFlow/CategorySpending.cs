namespace CashPrism.Application.CashFlow;

/// <summary>What was spent in one main category, in whole cents.</summary>
/// <param name="Category">Finanzguru's main category.</param>
/// <param name="SpendingInCents">What went out, as a positive number.</param>
public sealed record CategorySpending(string Category, long SpendingInCents);
