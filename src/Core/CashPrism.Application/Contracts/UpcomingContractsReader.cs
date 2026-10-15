using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// Reads the <see cref="UpcomingContracts"/>: the next booking of every
/// contract expected within <see cref="DaysAhead"/> days after the export
/// date, and every overdue one.
/// </summary>
/// <remarks>
/// A booking expected before the export date but still within
/// <see cref="Contract.OverdueToleranceInDays"/> is listed as well: it is due
/// and has not come, it just is not late yet. One whose overdue marker was
/// dismissed is not listed — the person said it needs no attention.
/// </remarks>
public sealed class UpcomingContractsReader
{
    /// <summary>How many days after the export date the list looks ahead.</summary>
    public const int DaysAhead = 30;

    private readonly ContractForecastReader reader;

    /// <summary>Creates the use case.</summary>
    /// <param name="reader">Reads the contracts with their forecasts.</param>
    public UpcomingContractsReader(ContractForecastReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        this.reader = reader;
    }

    /// <summary>
    /// Reads the list, or <see langword="null"/> when nothing was imported.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<UpcomingContracts?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (await reader.ReadAsync(cancellationToken) is not { } forecasts)
        {
            return null;
        }

        var currentTo = forecasts.CurrentTo;
        var from = currentTo.AddDays(-Contract.OverdueToleranceInDays);
        var to = currentTo.AddDays(DaysAhead);

        return new UpcomingContracts(
            currentTo,
            [.. forecasts.Contracts
                .Where(forecast => forecast.DaysOverdue is not null
                    || (forecast.NextBookingOn is { } next && next >= from && next <= to))
                .OrderByDescending(forecast => forecast.DaysOverdue ?? 0)
                .ThenBy(forecast => forecast.NextBookingOn)
                .ThenBy(forecast => forecast.Contract.Counterparty, StringComparer.CurrentCulture)]);
    }
}
