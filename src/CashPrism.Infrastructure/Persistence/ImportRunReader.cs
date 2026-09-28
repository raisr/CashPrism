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

        // Nothing read here is written back, and the tracker would otherwise hold
        // every run of every page a circuit ever looked at.
        var runs = context.ImportRuns.AsNoTracking();

        var total = await runs.CountAsync(cancellationToken);

        // Ordering by ImportedAt is only translatable because the column stores a
        // UTC DateTime rather than the DateTimeOffset the model carries — see
        // ImportRunConfiguration, which is where that is decided and why.
        //
        // The id is the second key for the same reason the booking list has one:
        // two runs can share an import time, and a tie left unbroken is how a
        // pager shows one run twice and another never.
        var page = await runs
            .OrderByDescending(run => run.ImportedAt)
            .ThenByDescending(run => run.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new Page<ImportRun>(page, total);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return context.ImportRuns.CountAsync(cancellationToken);
    }
}
