namespace CashPrism.Application.CashFlow;

/// <summary>Spending per main category over a run of complete months.</summary>
/// <param name="Months">The first day of each month of the period, oldest first.</param>
/// <param name="AverageInCents">What went out per month on average over all categories, rounded to the cent.</param>
/// <param name="Categories">
/// Every category that cost something in the period or in the one before it,
/// largest average first.
/// </param>
public sealed record SpendingPeriod(
    IReadOnlyList<DateOnly> Months,
    long AverageInCents,
    IReadOnlyList<CategoryComparison> Categories);
