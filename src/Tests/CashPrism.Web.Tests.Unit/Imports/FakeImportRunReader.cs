using CashPrism.Application.Imports;
using CashPrism.Application.Paging;
using CashPrism.Domain.Imports;

namespace CashPrism.Web.Tests.Unit.Imports;

/// <summary>
/// Stands in for the recorded import runs, holding them newest first as the
/// real reader hands them out.
/// </summary>
public sealed class FakeImportRunReader : IImportRunReader
{
    private readonly List<ImportRun> runs = [];

    /// <summary>
    /// Records a completed run that happened at <paramref name="importedAt"/>,
    /// as the newest one.
    /// </summary>
    public ImportRun Add(
        DateTimeOffset importedAt,
        string fileName = "export.xlsx",
        DateOnly? exportedOn = null,
        int rowsRead = 0,
        int inserted = 0,
        int updated = 0)
    {
        var run = new ImportRun(
            Guid.NewGuid(),
            fileName,
            "3b8f1c2d4e5a6b7c8d9e0f1a2b3c4d5e6f708192a3b4c5d6e7f8091a2b3c4d5e",
            "20260907_Export_Alle_Buchungen",
            exportedOn,
            importedAt);
        run.Complete(rowsRead, inserted, updated, bookingsUnchanged: 0);
        runs.Insert(0, run);

        return run;
    }

    public Task<Page<ImportRun>> ReadPageAsync(PageRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new Page<ImportRun>([.. runs.Skip(request.Skip).Take(request.Take)], runs.Count));

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(runs.Count);
}
