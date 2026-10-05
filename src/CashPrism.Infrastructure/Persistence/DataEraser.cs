using CashPrism.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Empties the three tables an import writes and compacts the SQLite file.
/// </summary>
/// <remarks>
/// <para>
/// The deletes run in one transaction, so a failure leaves the data as it was
/// rather than bookings without the runs they came from. Raw rows go first:
/// they point at their import run, and the delete must not depend on the
/// cascade to get the order right.
/// </para>
/// <para>
/// SQLite runs with its default <c>auto_vacuum = NONE</c> here, so a delete
/// only moves the pages onto the file's free list and the file keeps its size.
/// <c>VACUUM</c> rebuilds the database without them. It cannot run inside a
/// transaction, which is why it is a second step after the commit; at the size
/// of a household's bookings it takes a moment.
/// </para>
/// <para>
/// EF Core creates SQLite databases in WAL mode, where <c>VACUUM</c> writes the
/// rebuilt pages into the write-ahead log, and the file only shrinks once they
/// are checkpointed back. A <c>TRUNCATE</c> checkpoint does that and empties
/// the log as well, so the space is back on disk when the call returns.
/// </para>
/// </remarks>
public sealed class DataEraser : IDataEraser
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the eraser.
    /// </summary>
    /// <param name="context">The context whose tables are emptied.</param>
    public DataEraser(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task EraseAllAsync(CancellationToken cancellationToken = default)
    {
        await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
        {
            await context.RawRows.ExecuteDeleteAsync(cancellationToken);
            await context.Bookings.ExecuteDeleteAsync(cancellationToken);
            await context.ImportRuns.ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        // Not cancellable once the data is gone: the file is consistent either
        // way, and a half-hearted compaction buys nothing.
        await context.Database.ExecuteSqlRawAsync("VACUUM", CancellationToken.None);
        await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE)", CancellationToken.None);

        // The change tracker may still hold entities that no longer exist.
        context.ChangeTracker.Clear();
    }
}
