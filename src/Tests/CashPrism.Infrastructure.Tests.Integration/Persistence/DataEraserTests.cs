using System.Text;
using CashPrism.Application.Accounts;
using CashPrism.Application.Imports;
using CashPrism.Domain.Accounts;
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

    private static async Task EraseAsync(ThrowawayDatabase database, bool includeAccountGroups = false)
    {
        await using var context = database.CreateContext();

        await new DataEraser(context).EraseAllAsync(includeAccountGroups);
    }

    /// <summary>
    /// Stores two groups, <c>Eigene</c> on top and <c>Gemeinsam</c> below, with
    /// <paramref name="shared"/> in the second and every other account in the first.
    /// </summary>
    private static async Task GroupAsync(ThrowawayDatabase database, IReadOnlyList<string> accounts, string shared)
    {
        var grouping = AccountGrouping.Start(Guid.NewGuid(), "Eigene");
        var second = Guid.NewGuid();
        grouping.Add(second, "Gemeinsam");
        grouping.Place(accounts);
        grouping.Assign(shared, second);

        await using var context = database.CreateContext();
        await new AccountGroupStore(context).SaveAsync(grouping);
    }

    private static async Task<AccountGrouping?> LoadGroupsAsync(ThrowawayDatabase database)
    {
        await using var context = database.CreateContext();

        return await new AccountGroupStore(context).LoadAsync();
    }

    private static async Task<IReadOnlyList<string>> AccountsAsync(ThrowawayDatabase database)
    {
        await using var context = database.CreateContext();
        var balances = await new AccountBalanceReader(context).ReadLatestAsync();

        return [.. balances.Select(balance => balance.AccountReference).Order(StringComparer.Ordinal)];
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
        public async Task Leaves_No_Account_Balance()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database);

            await EraseAsync(database);

            await using var context = database.CreateContext();
            Assert.Equal(0, await context.AccountBalances.CountAsync());
        }

        [Fact]
        public async Task Keeps_The_Account_Groups_Unless_Asked()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await GroupAsync(database, ["DE02120300000000202051"], shared: "DE02120300000000202051");

            await EraseAsync(database, includeAccountGroups: false);

            var groups = await LoadGroupsAsync(database);
            Assert.Equal(["Eigene", "Gemeinsam"], groups!.Groups.Select(group => group.Name));
        }

        [Fact]
        public async Task Deletes_The_Account_Groups_When_Asked()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await GroupAsync(database, ["DE02120300000000202051"], shared: "DE02120300000000202051");

            await EraseAsync(database, includeAccountGroups: true);

            Assert.Null(await LoadGroupsAsync(database));
        }

        [Fact]
        public async Task Puts_An_Account_Back_In_Its_Group_After_The_Demo_Export_Is_Imported_Again()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();
            var accounts = await AccountsAsync(database);
            var shared = accounts[^1];
            await GroupAsync(database, accounts, shared);

            await EraseAsync(database, includeAccountGroups: false);
            await DemoSampleDatabase.ImportAsync(database);

            var groups = await LoadGroupsAsync(database);
            groups!.Place(await AccountsAsync(database));
            Assert.Equal([shared], groups.AccountsIn(groups.Groups[1].Id));
        }

        [Fact]
        public async Task Starts_Over_With_The_Single_Group_Konten_When_Asked()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();
            var accounts = await AccountsAsync(database);
            await GroupAsync(database, accounts, shared: accounts[^1]);

            await EraseAsync(database, includeAccountGroups: true);
            await DemoSampleDatabase.ImportAsync(database);

            await using var context = database.CreateContext();
            var groups = await new AccountGroupEditor(new AccountBalanceReader(context), new AccountGroupStore(context))
                .ReadAsync("Konten");
            var group = Assert.Single(groups);
            Assert.Equal("Konten", group.Name);
            Assert.Equal(accounts.Count, group.Accounts.Count);
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
