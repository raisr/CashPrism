using CashPrism.Application.Imports;
using CashPrism.Application.Paging;
using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Reads pages of import runs out of the database.
/// </summary>
public sealed class ImportRunReader : IImportRunReader
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    /// <param name="context">The database.</param>
    public ImportRunReader(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<Page<ImportRun>> ReadPageAsync(
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Read whole and ordered here rather than by the database, which is the
        // one thing this reader does that the booking reader does not.
        // ImportedAt is a DateTimeOffset, and SQLite has no type that orders one:
        // EF Core refuses to translate the ORDER BY rather than return a wrong
        // order, and no expression over the property translates either. What
        // makes that affordable is the size of the set — one row per imported
        // file, and a person imports a file at a time. Storing the instant in a
        // form SQLite can order is a change to the schema, and this reader is
        // where it would pay off first.
        //
        // Nothing read here is written back, and the tracker would otherwise hold
        // every run of every page a circuit ever looked at.
        var runs = await context.ImportRuns
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // The id is the second key for the same reason the booking list has one:
        // two runs can share an import time, and a tie left unbroken is how a
        // pager shows one run twice and another never.
        var page = runs
            .OrderByDescending(run => run.ImportedAt)
            .ThenByDescending(run => run.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return new Page<ImportRun>(page, runs.Count);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return context.ImportRuns.CountAsync(cancellationToken);
    }
}
