namespace CashPrism.Application.CashFlow;

/// <summary>
/// What the analysis shows: spending per main category over the complete
/// months the stored data covers, enough of them to compare the longest period
/// offered with the one before it.
/// </summary>
/// <param name="CurrentTo">The date the stored data is current to.</param>
/// <param name="Months">
/// <see cref="SpendingAnalysisReader.MonthsRead"/> complete months, oldest
/// first. The last one is the month before the one the data is current to.
/// </param>
/// <param name="Categories">
/// Every category that cost something in those months, ranked by what it cost
/// over the longest period offered, largest first.
/// </param>
public sealed record SpendingAnalysis(
    DateOnly CurrentTo,
    IReadOnlyList<DateOnly> Months,
    IReadOnlyList<CategoryHistory> Categories)
{
    /// <summary>
    /// The latest <paramref name="months"/> months, each category compared with
    /// the <paramref name="months"/> months before them.
    /// </summary>
    /// <param name="months">
    /// How long the period is, at most <see cref="SpendingAnalysisReader.LongestPeriod"/>.
    /// </param>
    public SpendingPeriod Compare(int months)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(months, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(months, SpendingAnalysisReader.LongestPeriod);

        var start = Months.Count - months;

        var periods = Categories
            .Select((history, rank) => (
                History: history,
                Rank: rank,
                Period: history.MonthlySpendingInCents.Skip(start).ToList(),
                Before: history.MonthlySpendingInCents.Skip(start - months).Take(months).Sum()))
            .ToList();

        var total = periods.Sum(category => category.Period.Sum());

        var categories = periods
            .Where(category => category.Period.Sum() > 0 || category.Before > 0)
            .Select(category =>
            {
                var sum = category.Period.Sum();

                return new CategoryComparison(
                    category.History.Category,
                    category.Rank,
                    category.Period,
                    Average(sum, months),
                    total == 0 ? 0 : (decimal)sum / total,
                    category.Before == 0 ? null : (decimal)(sum - category.Before) / category.Before);
            })
            .OrderByDescending(category => category.AverageInCents)
            .ThenBy(category => category.Rank)
            .ToList();

        return new SpendingPeriod(Months.Skip(start).ToList(), Average(total, months), categories);
    }

    private static long Average(long sumInCents, int months)
        => (long)Math.Round((decimal)sumInCents / months, MidpointRounding.AwayFromZero);
}
