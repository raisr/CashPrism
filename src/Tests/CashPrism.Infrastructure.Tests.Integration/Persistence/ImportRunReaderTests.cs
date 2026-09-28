using CashPrism.Application.Paging;
using CashPrism.Domain.Imports;
using CashPrism.Infrastructure.Persistence;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The read side of the list of past imports, against a real SQLite file.
/// </summary>
public sealed class ImportRunReaderTests
{
    private static readonly DateTimeOffset AnImportTime = new(2026, 9, 7, 18, 0, 0, TimeSpan.Zero);

    private static ImportRun CreateRun(
        Guid id,
        DateTimeOffset? importedAt = null,
        string fileName = "20260907_Export_Alle_Buchungen.xlsx",
        string fileHash = "3b8f1c")
        => new(
            id,
            fileName,
            fileHash,
            sheetName: "20260907_Export_Alle_Buchungen",
            new DateOnly(2026, 9, 7),
            importedAt ?? AnImportTime);

    /// <summary>An id that sorts in the same order as its ordinal.</summary>
    private static Guid Id(int ordinal) => new($"00000000-0000-0000-0000-{ordinal:D12}");

    private static async Task SeedAsync(ThrowawayDatabase database, params ImportRun[] runs)
    {
        await using var context = database.CreateContext();

        context.ImportRuns.AddRange(runs);

        await context.SaveChangesAsync();
    }

    private static ImportRunReader CreateReader(ThrowawayDatabase database, out CashPrismDbContext context)
    {
        context = database.CreateContext();

        return new ImportRunReader(context);
    }

    public sealed class CountAsync
    {
        [Fact]
        public async Task Counts_Nothing_In_An_Empty_Database()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            Assert.Equal(0, await reader.CountAsync());
        }

        [Fact]
        public async Task Counts_Every_Recorded_Run()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, CreateRun(Id(1)), CreateRun(Id(2)), CreateRun(Id(3)));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            Assert.Equal(3, await reader.CountAsync());
        }
    }

    public sealed class ReadPageAsync
    {
        private static async Task<IReadOnlyList<Guid>> IdsAsync(ThrowawayDatabase database, PageRequest request)
        {
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(request);

            return [.. page.Items.Select(run => run.Id)];
        }

        [Fact]
        public async Task Puts_The_Newest_Run_First()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateRun(Id(1), importedAt: AnImportTime.AddDays(-7)),
                CreateRun(Id(2), importedAt: AnImportTime),
                CreateRun(Id(3), importedAt: AnImportTime.AddDays(-2)));

            var ids = await IdsAsync(database, new PageRequest(Skip: 0, Take: 25));

            Assert.Equal([Id(2), Id(3), Id(1)], ids);
        }

        [Fact]
        public async Task Hands_Out_Only_The_Slice_It_Was_Asked_For()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                [.. Enumerable.Range(1, 10).Select(n => CreateRun(Id(n), importedAt: AnImportTime.AddDays(-n)))]);

            var ids = await IdsAsync(database, new PageRequest(Skip: 3, Take: 2));

            // Newest first, so the fourth and fifth entries are the runs four and
            // five days back.
            Assert.Equal([Id(4), Id(5)], ids);
        }

        /// <summary>
        /// Two runs can share an import time — a person importing two exports in
        /// the same second. Without the id as a second sort key SQLite may order
        /// them differently for every query, and a pager then shows one run twice
        /// and another never.
        /// </summary>
        [Fact]
        public async Task Pages_Do_Not_Overlap_When_Every_Run_Shares_An_Import_Time()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, [.. Enumerable.Range(1, 60).Select(n => CreateRun(Id(n)))]);

            var first = await IdsAsync(database, new PageRequest(Skip: 0, Take: 25));
            var second = await IdsAsync(database, new PageRequest(Skip: 25, Take: 25));
            var third = await IdsAsync(database, new PageRequest(Skip: 50, Take: 25));

            Assert.Equal(60, first.Concat(second).Concat(third).Distinct().Count());
        }

        [Fact]
        public async Task Reports_How_Many_Runs_There_Are_Rather_Than_How_Many_It_Returned()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, [.. Enumerable.Range(1, 60).Select(n => CreateRun(Id(n)))]);
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new PageRequest(Skip: 0, Take: 25));

            Assert.Equal(25, page.Items.Count);
            Assert.Equal(60, page.TotalCount);
        }

        [Fact]
        public async Task Hands_Back_An_Empty_Page_Beyond_The_Last_Run()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, CreateRun(Id(1)));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new PageRequest(Skip: 100, Take: 25));

            Assert.Empty(page.Items);
            Assert.Equal(1, page.TotalCount);
        }

        [Fact]
        public async Task Reads_A_Run_Back_With_The_Fields_The_List_Shows()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var run = CreateRun(Id(1), fileName: "export.xlsx", fileHash: "3b8f1c2d4e5a6b7c");
            run.Complete(rowsRead: 6327, bookingsInserted: 12, bookingsUpdated: 3, bookingsUnchanged: 6312);
            await SeedAsync(database, run);
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new PageRequest(Skip: 0, Take: 25));

            var read = Assert.Single(page.Items);
            Assert.Equal("export.xlsx", read.FileName);
            Assert.Equal("3b8f1c2d4e5a6b7c", read.FileHash);
            Assert.Equal(new DateOnly(2026, 9, 7), read.ExportedOn);
            Assert.Equal(AnImportTime, read.ImportedAt);
            Assert.Equal(6327, read.RowsRead);
            Assert.Equal(12, read.BookingsInserted);
            Assert.Equal(3, read.BookingsUpdated);
            Assert.Equal(6312, read.BookingsUnchanged);
        }

        [Fact]
        public async Task Reads_Back_A_Run_Whose_Sheet_Name_Carried_No_Export_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            // Built here rather than through the helper: the helper fills in an
            // export date, and this test is about the run that has none.
            await SeedAsync(
                database,
                new ImportRun(
                    Id(1),
                    fileName: "buchungen.xlsx",
                    fileHash: "3b8f1c",
                    sheetName: "Tabelle1",
                    exportedOn: null,
                    AnImportTime));

            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new PageRequest(Skip: 0, Take: 25));

            Assert.Null(Assert.Single(page.Items).ExportedOn);
        }
    }
}
