using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Keeps the dismissed overdue markers in their table, at most one per
/// contract.
/// </summary>
public sealed class DismissedOverdueStore : IDismissedOverdueStore
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the store.
    /// </summary>
    /// <param name="context">The database.</param>
    public DismissedOverdueStore(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DismissedOverdue>> ReadAllAsync(CancellationToken cancellationToken = default)
        => await context.DismissedOverdues.AsNoTracking().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task DismissAsync(DismissedOverdue dismissal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dismissal);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.DismissedOverdues
            .Where(stored => stored.ContractId == dismissal.ContractId)
            .ExecuteDeleteAsync(cancellationToken);

        context.DismissedOverdues.Add(dismissal);
        await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // Nothing written here is read back through the tracker.
        context.ChangeTracker.Clear();
    }
}
