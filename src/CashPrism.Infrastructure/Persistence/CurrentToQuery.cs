using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// The date the stored data is current to, read the same way by every reader
/// that measures something from it.
/// </summary>
internal static class CurrentToQuery
{
    /// <summary>
    /// The latest export date among the import runs, or the day of the latest
    /// import where no run carries an export date. <see langword="null"/> when
    /// nothing was imported.
    /// </summary>
    /// <param name="context">The database.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public static async Task<DateOnly?> ReadAsync(CashPrismDbContext context, CancellationToken cancellationToken)
    {
        var runs = context.ImportRuns.AsNoTracking();

        var exportedOn = await runs.MaxAsync(run => run.ExportedOn, cancellationToken);

        if (exportedOn.HasValue)
        {
            return exportedOn;
        }

        // ImportedAt is stored as a UTC DateTime, which is what makes the
        // order translatable — see ImportRunConfiguration.
        var importedAt = await runs
            .OrderByDescending(run => run.ImportedAt)
            .Select(run => (DateTimeOffset?)run.ImportedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return importedAt is { } latest ? DateOnly.FromDateTime(latest.UtcDateTime) : null;
    }
}
