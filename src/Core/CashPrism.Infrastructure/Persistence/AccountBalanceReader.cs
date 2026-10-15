using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Reads the balances of the latest import run out of the database.
/// </summary>
public sealed class AccountBalanceReader : IAccountBalanceReader
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    /// <param name="context">The database.</param>
    public AccountBalanceReader(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountBalance>> ReadLatestAsync(CancellationToken cancellationToken = default)
    {
        // The latest run is picked in memory: "later" falls back from the export
        // date to the import time where a run has no export date, which no single
        // ORDER BY expresses. One row per import is few enough to load whole.
        var runs = await context.ImportRuns
            .AsNoTracking()
            .Where(run => run.IsComplete)
            .ToListAsync(cancellationToken);

        if (runs.Count == 0)
        {
            return [];
        }

        var latest = runs.Aggregate((best, run) => run.IsLaterThan(best) ? run : best);

        return await context.AccountBalances
            .AsNoTracking()
            .Where(balance => balance.ImportRunId == latest.Id)
            .ToListAsync(cancellationToken);
    }
}
