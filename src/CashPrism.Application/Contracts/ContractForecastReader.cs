namespace CashPrism.Application.Contracts;

/// <summary>
/// Reads every contract with its next booking and whether it is overdue,
/// measured against the date the export is current to, with the markers the
/// person dismissed taken out.
/// </summary>
/// <remarks>
/// A dismissal holds for the one expected date it was made for. Once the
/// contract books again, or that booking is followed by another overdue one,
/// the date differs and the marker shows again.
/// </remarks>
public sealed class ContractForecastReader
{
    private readonly IContractReader reader;
    private readonly IDismissedOverdueStore dismissals;

    /// <summary>Creates the reader.</summary>
    /// <param name="reader">Reads the stored contracts.</param>
    /// <param name="dismissals">Reads the dismissed overdue markers.</param>
    public ContractForecastReader(IContractReader reader, IDismissedOverdueStore dismissals)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(dismissals);

        this.reader = reader;
        this.dismissals = dismissals;
    }

    /// <summary>
    /// Reads the forecasts, or <see langword="null"/> when nothing was
    /// imported and there is nothing to forecast from.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<ContractForecasts?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (await reader.ReadCurrentToAsync(cancellationToken) is not { } currentTo)
        {
            return null;
        }

        var contracts = await reader.ReadAllAsync(cancellationToken);
        var dismissed = (await dismissals.ReadAllAsync(cancellationToken))
            .Select(dismissal => (dismissal.ContractId, dismissal.ExpectedOn))
            .ToHashSet();

        return new ContractForecasts(
            currentTo,
            [.. contracts.Select(contract =>
            {
                var next = contract.NextBookingOn(currentTo);
                var overdue = next is { } expected && dismissed.Contains((contract.ContractId, expected))
                    ? null
                    : contract.DaysOverdue(currentTo);

                return new ContractForecast(contract, next, overdue);
            })]);
    }
}
