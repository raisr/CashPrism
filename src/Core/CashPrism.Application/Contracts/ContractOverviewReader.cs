namespace CashPrism.Application.Contracts;

/// <summary>
/// Reads the <see cref="ContractOverview"/>: the contracts sorted into the
/// sections the contracts page shows, each with its forecast, and what they
/// cost a month.
/// </summary>
/// <remarks>
/// Every contract lands in exactly one section. One whose bookings have
/// stopped is presumably ended, whatever else it is; of the rest, a transfer
/// is a transfer even where money comes in, then income, and what remains is
/// spending, grouped by interval.
/// </remarks>
public sealed class ContractOverviewReader
{
    private readonly ContractForecastReader reader;

    /// <summary>Creates the use case.</summary>
    /// <param name="reader">Reads the contracts with their forecasts.</param>
    public ContractOverviewReader(ContractForecastReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        this.reader = reader;
    }

    /// <summary>
    /// Reads the overview, or <see langword="null"/> when nothing was imported
    /// and there is nothing to show.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<ContractOverview?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (await reader.ReadAsync(cancellationToken) is not { } forecasts)
        {
            return null;
        }

        var currentTo = forecasts.CurrentTo;
        var contracts = forecasts.Contracts;

        var ended = contracts.Where(forecast => forecast.Contract.IsPresumablyEnded(currentTo)).ToList();
        var running = contracts.Except(ended).ToList();
        var transfers = running.Where(forecast => forecast.Contract.IsTransfer).ToList();
        var income = running.Where(forecast => !forecast.Contract.IsTransfer && forecast.Contract.IsIncome).ToList();
        var spending = running.Where(forecast => !forecast.Contract.IsTransfer && !forecast.Contract.IsIncome).ToList();

        var byInterval = spending
            .GroupBy(forecast => forecast.Contract.Interval, StringComparer.Ordinal)
            .OrderBy(group => group.First().Contract.MonthsPerPayment ?? int.MaxValue)
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ContractGroup(group.Key, LargestFirst(group)))
            .ToList();

        var monthlyCostInCents = -spending.Sum(forecast => forecast.Contract.MonthlyAmountInCents ?? 0);

        return new ContractOverview(
            currentTo,
            monthlyCostInCents,
            byInterval,
            new ContractGroup(null, LargestFirst(income)),
            new ContractGroup(null, LargestFirst(transfers)),
            new ContractGroup(null, [.. ended.OrderByDescending(forecast => forecast.Contract.NewestBookedOn)]));
    }

    private static List<ContractForecast> LargestFirst(IEnumerable<ContractForecast> contracts)
        => [.. contracts
            .OrderByDescending(forecast => Math.Abs(forecast.Contract.AmountInCents))
            .ThenBy(forecast => forecast.Contract.Counterparty, StringComparer.CurrentCulture)];
}
