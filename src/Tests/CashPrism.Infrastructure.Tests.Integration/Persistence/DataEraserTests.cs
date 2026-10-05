using System.Text;
using CashPrism.Application.Imports;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Imports;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class DataEraserTests
{
    private const string AFileContent = "an export";

    private static readonly DateTimeOffset AnImportTime = new(2026, 9, 7, 18, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Imports a file of <paramref name="bookings"/> bookings through the real
    /// store, each with a raw row the size of a real one (about 900 bytes).
    /// </summary>
    private static async Task<ImportResult> ImportAsync(ThrowawayDatabase database, int bookings = 3)
    {
        var rows = Enumerable.Range(1, bookings)
            .Select(ordinal => (Fingerprint: ordinal.ToString("D40", null), Payload: new string('x', 900) + ordinal))
            .ToArray();
        await using var context = database.CreateContext();
        var importer = new Importer(
            FakeImportSource.Carrying(new DateOnly(2026, 9, 7), rows),
            new ImportStore(context),
            new FixedClock(AnImportTime));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(AFileContent));

        return await importer.ImportAsync(stream, fileName: "20260907_Export_Alle_Buchungen.xlsx");
    }

    private static async Task EraseAsync(ThrowawayDatabase database)
    {
        await using var context = database.CreateContext();

        await new DataEraser(context).EraseAllAsync();
    }

    public sealed class EraseAllAsync
    {
        [Fact]
        public async Task Leaves_No_Booking_Raw_Row_Or_Import_Run()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database);

            await EraseAsync(database);

            await using var context = database.CreateContext();
            Assert.Equal(
                (0, 0, 0),
                (await context.Bookings.CountAsync(), await context.RawRows.CountAsync(), await context.ImportRuns.CountAsync()));
        }

        [Fact]
        public async Task Gives_The_Space_Back_To_The_Disk()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database, bookings: 2_000);
            var before = database.FileSizeInBytes;

            await EraseAsync(database);

            Assert.True(database.FileSizeInBytes < before, $"{database.FileSizeInBytes} bytes after, {before} before");
        }

        [Fact]
        public async Task Lets_The_Same_File_Be_Imported_Again()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database);

            await EraseAsync(database);
            var again = await ImportAsync(database);

            Assert.Equal(ImportOutcome.Imported, again.Outcome);
        }

        [Fact]
        public async Task Keeps_The_Schema()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database);

            await EraseAsync(database);

            await using var context = database.CreateContext();
            Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        }
    }
}
