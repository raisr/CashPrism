using CashPrism.Application.CashFlow;
using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Reads how the balance of every account developed, walked back from the
/// balance taken at import by <see cref="BalanceHistoryRule"/>.
/// </summary>
public sealed class AccountBalanceHistoryReader
{
    /// <summary>
    /// How many points a history has at most: the ends of the twelve complete
    /// months before the running one, and the day the data is current to.
    /// </summary>
    public const int PointsShown = 13;

    private readonly IAccountBalanceReader balances;
    private readonly IBalanceMovementReader movements;
    private readonly ICashFlowReader cashFlow;

    /// <summary>Creates the use case.</summary>
    /// <param name="balances">Reads the balances taken at import.</param>
    /// <param name="movements">Reads what each booking contributes.</param>
    /// <param name="cashFlow">Tells the date the stored data is current to.</param>
    public AccountBalanceHistoryReader(
        IAccountBalanceReader balances,
        IBalanceMovementReader movements,
        ICashFlowReader cashFlow)
    {
        ArgumentNullException.ThrowIfNull(balances);
        ArgumentNullException.ThrowIfNull(movements);
        ArgumentNullException.ThrowIfNull(cashFlow);

        this.balances = balances;
        this.movements = movements;
        this.cashFlow = cashFlow;
    }

    /// <summary>
    /// The history of every account that has a balance, or none when nothing
    /// was imported.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<IReadOnlyList<AccountBalanceHistory>> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (await cashFlow.ReadCurrentToAsync(cancellationToken) is not { } currentTo)
        {
            return [];
        }

        var accounts = await balances.ReadLatestAsync(cancellationToken);
        var byAccount = (await movements.ReadAllAsync(cancellationToken))
            .ToLookup(movement => movement.AccountReference, StringComparer.Ordinal);

        var runningMonth = new DateOnly(currentTo.Year, currentTo.Month, 1);
        IReadOnlyList<DateOnly> days =
        [
            .. Enumerable.Range(0, PointsShown - 1)
                .Select(offset => runningMonth.AddMonths(offset - PointsShown + 2).AddDays(-1)),
            currentTo,
        ];

        return accounts
            .Select(account =>
            {
                var history = BalanceHistoryRule.For(account.BalanceInCents, currentTo, [.. byAccount[account.AccountReference]], days);
                var points = history.BalancesInCents
                    .Select((balance, index) => balance is { } cents ? new DatedBalance(days[index], cents) : null)
                    .OfType<DatedBalance>()
                    .ToList();

                return new AccountBalanceHistory(account.AccountReference, history.Holds, points);
            })
            .ToList();
    }
}
