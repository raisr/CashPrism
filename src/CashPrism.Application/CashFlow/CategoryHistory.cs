namespace CashPrism.Application.CashFlow;

/// <summary>What one main category cost month by month, in whole cents.</summary>
/// <param name="Category">Finanzguru's main category.</param>
/// <param name="MonthlySpendingInCents">
/// What went out in each month of <see cref="SpendingAnalysis.Months"/>, in the
/// same order, as positive numbers; zero for a month the category cost nothing.
/// </param>
public sealed record CategoryHistory(string Category, IReadOnlyList<long> MonthlySpendingInCents);
