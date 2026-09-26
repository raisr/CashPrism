using CashPrism.Application.Imports;
using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// The stored side of an import, on EF Core.
/// </summary>
/// <remarks>
/// Every read is untracked and every batch ends by clearing the change tracker.
/// An export carries the owner's whole history, so a tracker that kept every
/// entity of a run would grow into the tens of thousands — where its fixup turns
/// from a detail into the slowest part of the import.
/// </remarks>
public sealed class ImportStore : IImportStore
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the store.
    /// </summary>
    /// <param name="context">The database.</param>
    public ImportStore(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<ImportRun?> FindRunByFileHashAsync(
        string fileHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileHash);

        return await context.ImportRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(run => run.FileHash == fileHash, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddRunAsync(ImportRun run, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        context.ImportRuns.Add(run);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, StoredBookingState>> LoadStatesAsync(
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fingerprints);

        if (fingerprints.Count == 0)
        {
            return new Dictionary<string, StoredBookingState>(StringComparer.Ordinal);
        }

        var wanted = fingerprints as IList<string> ?? [.. fingerprints];

        // The run that wrote the booking wrote its raw row under the same key, so
        // the stored state comes back in one join rather than a scan for the
        // newest row per booking.
        var rows = await context.Bookings
            .AsNoTracking()
            .Where(booking => wanted.Contains(booking.Fingerprint))
            .Join(
                context.RawRows.AsNoTracking(),
                booking => new { RunId = booking.SourceImportRunId, booking.Fingerprint },
                rawRow => new { RunId = rawRow.ImportRunId, rawRow.Fingerprint },
                (booking, rawRow) => new
                {
                    booking.Fingerprint,
                    booking.SourceImportRunId,
                    rawRow.Json,
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count == 0)
        {
            return new Dictionary<string, StoredBookingState>(StringComparer.Ordinal);
        }

        var runIds = rows.Select(row => row.SourceImportRunId).Distinct().ToList();
        var runs = await context.ImportRuns
            .AsNoTracking()
            .Where(run => runIds.Contains(run.Id))
            .ToDictionaryAsync(run => run.Id, cancellationToken)
            .ConfigureAwait(false);

        var states = new Dictionary<string, StoredBookingState>(rows.Count, StringComparer.Ordinal);

        foreach (var row in rows)
        {
            states[row.Fingerprint] = new StoredBookingState(
                row.Fingerprint,
                runs[row.SourceImportRunId],
                row.Json);
        }

        return states;
    }

    /// <inheritdoc />
    public async Task ApplyAsync(ImportBatch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        context.Bookings.AddRange(batch.BookingsToInsert);
        context.Bookings.UpdateRange(batch.BookingsToUpdate);
        context.RawRows.AddRange(batch.RawRowsToStore);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
    }

    /// <inheritdoc />
    public async Task CompleteRunAsync(ImportRun run, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        // The run was written before the batches and the tracker has been cleared
        // since, so it comes back as a detached object carrying its final counts.
        context.ImportRuns.Update(run);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
    }
}
