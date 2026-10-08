namespace CashPrism.Application.CashFlow;

/// <summary>
/// Reads the <see cref="CashFlowOverview"/>. The months are counted back from
/// the date the stored data is current to, not from today: the overview
/// mirrors the latest export, and an export a few weeks old would otherwise
/// show its last month as empty.
/// </summary>
public sealed class CashFlowOverviewReader
{
    /// <summary>
    /// How many months the overview reads, running month included. The
    /// longest span the chart offers, so switching spans reads nothing again.
    /// </summary>
    public const int MonthsShown = 24;

    private readonly ICashFlowReader reader;

    /// <summary>Creates the use case.</summary>
    /// <param name="reader">Sums the stored bookings.</param>
    public CashFlowOverviewReader(ICashFlowReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        this.reader = reader;
    }

    /// <summary>
    /// Reads the overview, or <see langword="null"/> when nothing was imported
    /// and there is nothing to show.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<CashFlowOverview?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (await reader.ReadCurrentToAsync(cancellationToken) is not { } currentTo)
        {
            return null;
        }

        var runningMonth = new DateOnly(currentTo.Year, currentTo.Month, 1);
        var firstMonth = runningMonth.AddMonths(1 - MonthsShown);

        var stored = await reader.ReadMonthsAsync(firstMonth, runningMonth, cancellationToken);
        var months = Enumerable.Range(0, MonthsShown)
            .Select(offset => firstMonth.AddMonths(offset))
            .Select(month => stored.FirstOrDefault(flow => flow.Month == month) ?? new MonthlyCashFlow(month, 0, 0))
            .ToList();

        var categories = await reader.ReadSpendingByCategoryAsync(runningMonth.AddMonths(-1), cancellationToken);

        return new CashFlowOverview(currentTo, months, categories);
    }
}
