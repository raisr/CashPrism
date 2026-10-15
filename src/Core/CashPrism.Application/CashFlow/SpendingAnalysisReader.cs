using CashPrism.Domain.CashFlow;

namespace CashPrism.Application.CashFlow;

/// <summary>
/// Reads the <see cref="SpendingAnalysis"/>. Only complete months count: an
/// average over a month the export was taken in the middle of would come out
/// too low. The months are counted back from the date the stored data is
/// current to, not from today, for the reason the overview gives.
/// </summary>
public sealed class SpendingAnalysisReader
{
    /// <summary>The longest period the analysis compares, in months.</summary>
    public const int LongestPeriod = 12;

    /// <summary>
    /// How many complete months are read: the longest period and the one
    /// before it, so switching periods reads nothing again.
    /// </summary>
    public const int MonthsRead = 2 * LongestPeriod;

    private readonly ICashFlowReader reader;

    /// <summary>Creates the use case.</summary>
    /// <param name="reader">Sums the stored bookings.</param>
    public SpendingAnalysisReader(ICashFlowReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        this.reader = reader;
    }

    /// <summary>
    /// Reads the analysis, or <see langword="null"/> when nothing was imported
    /// and there is nothing to show.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<SpendingAnalysis?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (await reader.ReadCurrentToAsync(cancellationToken) is not { } currentTo)
        {
            return null;
        }

        var lastMonth = new DateOnly(currentTo.Year, currentTo.Month, 1).AddMonths(-1);
        var firstMonth = lastMonth.AddMonths(1 - MonthsRead);

        var months = Enumerable.Range(0, MonthsRead).Select(firstMonth.AddMonths).ToList();
        var nets = await reader.ReadCategoryNetsAsync(firstMonth, lastMonth, cancellationToken);

        // Ranked over the longest period alone, the same for every period
        // shown, so a category keeps its colour when the period is switched.
        var categories = nets
            .GroupBy(net => net.Category, StringComparer.Ordinal)
            .Select(category =>
            {
                var spendingByMonth = category.ToDictionary(
                    net => net.Month,
                    net => CashFlowRule.CategorySpendingInCents(net.NetInCents));

                return new CategoryHistory(
                    category.Key,
                    months.Select(month => spendingByMonth.GetValueOrDefault(month)).ToList());
            })
            .Where(history => history.MonthlySpendingInCents.Any(spending => spending > 0))
            .OrderByDescending(history => history.MonthlySpendingInCents.Skip(MonthsRead - LongestPeriod).Sum())
            .ThenByDescending(history => history.MonthlySpendingInCents.Sum())
            .ThenBy(history => history.Category, StringComparer.Ordinal)
            .ToList();

        return new SpendingAnalysis(currentTo, months, categories);
    }
}
