using CashPrism.Application.Imports;
using CashPrism.Application.Paging;
using CashPrism.Domain.Imports;

namespace CashPrism.Web.Tests.Unit.Layout;

/// <summary>
/// Stands in for the recorded import runs, holding them newest first as the
/// real reader hands them out.
/// </summary>
public sealed class FakeImportRunReader : IImportRunReader
{
    private readonly List<ImportRun> runs = [];

    /// <summary>Records a run that happened at <paramref name="importedAt"/>, as the newest one.</summary>
    public void Add(DateTimeOffset importedAt)
        => runs.Insert(0, new ImportRun(Guid.NewGuid(), "export.xlsx", "3b8f1c", "20260907_Export_Alle_Buchungen", null, importedAt));

    public Task<Page<ImportRun>> ReadPageAsync(PageRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new Page<ImportRun>([.. runs.Skip(request.Skip).Take(request.Take)], runs.Count));

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(runs.Count);
}
