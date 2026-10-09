using CashPrism.Application.CashFlow;

namespace CashPrism.TestSupport.CashFlow;

/// <summary>
/// Hands out fixed category sums and records which months were asked for, the
/// way the database reader would answer them.
/// </summary>
public sealed class FakeCashFlowReader(DateOnly? currentTo, IReadOnlyList<CategoryNet>? nets = null) : ICashFlowReader
{
    /// <summary>The months the last call asked for, or nothing before the first.</summary>
    public (DateOnly First, DateOnly Last)? MonthsAskedFor { get; private set; }

    /// <inheritdoc />
    public Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(currentTo);

    /// <inheritdoc />
    public Task<IReadOnlyList<CategoryNet>> ReadCategoryNetsAsync(
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default)
    {
        MonthsAskedFor = (firstMonth, lastMonth);

        return Task.FromResult(nets ?? []);
    }
}
