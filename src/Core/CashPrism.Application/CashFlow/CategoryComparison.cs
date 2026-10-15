namespace CashPrism.Application.CashFlow;

/// <summary>
/// What one main category cost in a period, set against the period of the
/// same length before it.
/// </summary>
/// <param name="Category">Finanzguru's main category.</param>
/// <param name="Rank">
/// The category's place in <see cref="SpendingAnalysis.Categories"/>, counted
/// from zero. It does not change with the period, so whatever is drawn by rank
/// keeps its look when the period is switched.
/// </param>
/// <param name="MonthlySpendingInCents">What went out in each month of the period, oldest first.</param>
/// <param name="AverageInCents">What went out per month on average, rounded to the cent.</param>
/// <param name="Share">
/// The category's part of everything that went out in the period, as a
/// fraction — 0.45 for 45 per cent.
/// </param>
/// <param name="Change">
/// How the spending changed against the period before, as a fraction.
/// <see langword="null"/> when the category cost nothing then.
/// </param>
public sealed record CategoryComparison(
    string Category,
    int Rank,
    IReadOnlyList<long> MonthlySpendingInCents,
    long AverageInCents,
    decimal Share,
    decimal? Change);
