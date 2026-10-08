using CashPrism.Application.CashFlow;

namespace CashPrism.TestSupport.CashFlow;

/// <summary>
/// Hands out fixed sums and records which months were asked for, the way the
/// database reader would answer them.
/// </summary>
public sealed class FakeCashFlowReader(
    DateOnly? currentTo,
    IReadOnlyList<MonthlyCashFlow>? months = null,
    IReadOnlyList<CategorySpending>? categories = null) : ICashFlowReader
{
    public (DateOnly First, DateOnly Last)? MonthsAskedFor { get; private set; }

    public DateOnly? CategoryMonthAskedFor { get; private set; }

    public Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(currentTo);

    public Task<IReadOnlyList<MonthlyCashFlow>> ReadMonthsAsync(
        DateOnly firstMonth,
        DateOnly lastMonth,
        CancellationToken cancellationToken = default)
    {
        MonthsAskedFor = (firstMonth, lastMonth);

        return Task.FromResult(months ?? []);
    }

    public Task<IReadOnlyList<CategorySpending>> ReadSpendingByCategoryAsync(
        DateOnly month,
        CancellationToken cancellationToken = default)
    {
        CategoryMonthAskedFor = month;

        return Task.FromResult(categories ?? []);
    }
}
