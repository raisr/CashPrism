using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// Reads the <see cref="ContractOverview"/>: the contracts sorted into the
/// sections the contracts page shows, and what they cost a month.
/// </summary>
/// <remarks>
/// Every contract lands in exactly one section. One whose bookings have
/// stopped is presumably ended, whatever else it is; of the rest, a transfer
/// is a transfer even where money comes in, then income, and what remains is
/// spending, grouped by interval.
/// </remarks>
public sealed class ContractOverviewReader
{
    private readonly IContractReader reader;

    /// <summary>Creates the use case.</summary>
    /// <param name="reader">Reads the stored contracts.</param>
    public ContractOverviewReader(IContractReader reader)
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
        if (await reader.ReadCurrentToAsync(cancellationToken) is not { } currentTo)
        {
            return null;
        }

        var contracts = await reader.ReadAllAsync(cancellationToken);

        var ended = contracts.Where(contract => contract.IsPresumablyEnded(currentTo)).ToList();
        var running = contracts.Except(ended).ToList();
        var transfers = running.Where(contract => contract.IsTransfer).ToList();
        var income = running.Where(contract => !contract.IsTransfer && contract.IsIncome).ToList();
        var spending = running.Where(contract => !contract.IsTransfer && !contract.IsIncome).ToList();

        var byInterval = spending
            .GroupBy(contract => contract.Interval, StringComparer.Ordinal)
            .OrderBy(group => group.First().MonthsPerPayment ?? int.MaxValue)
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ContractGroup(group.Key, LargestFirst(group)))
            .ToList();

        var monthlyCostInCents = -spending.Sum(contract => contract.MonthlyAmountInCents ?? 0);

        return new ContractOverview(
            currentTo,
            monthlyCostInCents,
            byInterval,
            new ContractGroup(null, LargestFirst(income)),
            new ContractGroup(null, LargestFirst(transfers)),
            new ContractGroup(null, [.. ended.OrderByDescending(contract => contract.NewestBookedOn)]));
    }

    private static List<Contract> LargestFirst(IEnumerable<Contract> contracts)
        => [.. contracts
            .OrderByDescending(contract => Math.Abs(contract.AmountInCents))
            .ThenBy(contract => contract.Counterparty, StringComparer.CurrentCulture)];
}
