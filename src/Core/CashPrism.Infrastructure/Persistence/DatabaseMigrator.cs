using CashPrism.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Applies the EF Core migrations to the SQLite file.
/// </summary>
public sealed class DatabaseMigrator : IDatabaseMigrator
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the migrator.
    /// </summary>
    /// <param name="context">The context whose migrations are applied.</param>
    public DatabaseMigrator(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public Task MigrateAsync(CancellationToken cancellationToken = default)
        => context.Database.MigrateAsync(cancellationToken);
}
