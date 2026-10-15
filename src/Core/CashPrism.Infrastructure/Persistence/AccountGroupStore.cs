using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Keeps the account groups and assignments in their two tables.
/// </summary>
/// <remarks>
/// A save replaces both tables rather than working out what changed: a
/// household has a handful of groups and a few dozen accounts, and a move
/// renumbers the positions of its neighbours anyway.
/// </remarks>
public sealed class AccountGroupStore : IAccountGroupStore
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the store.
    /// </summary>
    /// <param name="context">The database.</param>
    public AccountGroupStore(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<AccountGrouping?> LoadAsync(CancellationToken cancellationToken = default)
    {
        // One after the other: the context takes one query at a time.
        var groups = await context.AccountGroups.AsNoTracking().ToListAsync(cancellationToken);

        if (groups.Count == 0)
        {
            return null;
        }

        var assignments = await context.AccountAssignments.AsNoTracking().ToListAsync(cancellationToken);

        return new AccountGrouping(groups, assignments);
    }

    /// <inheritdoc />
    public async Task SaveAsync(AccountGrouping grouping, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grouping);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.AccountAssignments.ExecuteDeleteAsync(cancellationToken);
        await context.AccountGroups.ExecuteDeleteAsync(cancellationToken);

        context.AccountGroups.AddRange(grouping.Groups);
        context.AccountAssignments.AddRange(grouping.Assignments);
        await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // Nothing written here is read back through the tracker: the next load
        // asks the database.
        context.ChangeTracker.Clear();
    }
}
