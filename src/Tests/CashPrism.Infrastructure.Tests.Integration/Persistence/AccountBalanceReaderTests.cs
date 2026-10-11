using CashPrism.Domain.Accounts;
using CashPrism.Domain.Imports;
using CashPrism.Infrastructure.Persistence;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class AccountBalanceReaderTests
{
    private const string Giro = "DE02120300000000202051";

    /// <summary>
    /// Stores a run with one balance for <see cref="Giro"/>, the way an import
    /// would, completed or not.
    /// </summary>
    private static async Task<Guid> SeedAsync(
        ThrowawayDatabase database,
        DateOnly? exportedOn,
        DateTimeOffset importedAt,
        long balanceInCents,
        bool complete = true)
    {
        var run = new ImportRun(
            Guid.NewGuid(),
            fileName: "export.xlsx",
            fileHash: Guid.NewGuid().ToString("N"),
            sheetName: "Export_Alle_Buchungen",
            exportedOn,
            importedAt);

        await using var context = database.CreateContext();
        var store = new ImportStore(context);
        await store.AddRunAsync(run);

        if (complete)
        {
            run.Complete(rowsRead: 1, bookingsInserted: 1, bookingsUpdated: 0, bookingsUnchanged: 0);
            await store.CompleteRunAsync(
                run,
                [new AccountBalance(run.Id, Giro, "Girokonto", balanceInCents, "EUR", importedAt.UtcDateTime)]);
        }

        return run.Id;
    }

    private static async Task<IReadOnlyList<AccountBalance>> ReadAsync(ThrowawayDatabase database)
    {
        await using var context = database.CreateContext();

        return await new AccountBalanceReader(context).ReadLatestAsync();
    }

    public sealed class ReadLatestAsync
    {
        private static readonly DateTimeOffset AnImportTime = new(2026, 10, 5, 18, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task Reads_Nothing_Before_The_First_Import()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            Assert.Empty(await ReadAsync(database));
        }

        [Fact]
        public async Task Stays_On_The_Newer_Export_When_An_Older_One_Is_Imported_After_It()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, new DateOnly(2026, 10, 5), AnImportTime, balanceInCents: 150_000);
            await SeedAsync(database, new DateOnly(2026, 9, 5), AnImportTime.AddDays(1), balanceInCents: 90_000);

            var balance = Assert.Single(await ReadAsync(database));

            Assert.Equal(150_000, balance.BalanceInCents);
        }

        [Fact]
        public async Task Takes_The_Later_Import_When_A_Run_Has_No_Export_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, new DateOnly(2026, 10, 5), AnImportTime, balanceInCents: 150_000);
            await SeedAsync(database, exportedOn: null, AnImportTime.AddDays(1), balanceInCents: 170_000);

            var balance = Assert.Single(await ReadAsync(database));

            Assert.Equal(170_000, balance.BalanceInCents);
        }

        [Fact]
        public async Task Ignores_A_Run_That_Never_Completed()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, new DateOnly(2026, 10, 5), AnImportTime, balanceInCents: 150_000);
            await SeedAsync(database, new DateOnly(2026, 10, 6), AnImportTime.AddDays(1), balanceInCents: 0, complete: false);

            var balance = Assert.Single(await ReadAsync(database));

            Assert.Equal(150_000, balance.BalanceInCents);
        }

        // Expected figures measured from samples/demo-export.xlsx outside
        // CashPrism: per account, the Kontostand of its topmost row not dated
        // after 2026-10-01, the date in the sheet name.
        [Fact]
        public async Task Reads_The_Balance_Of_An_Account_Of_The_Demo_Export()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();

            var balances = await ReadAsync(database);

            Assert.Equal(460_373, balances.Single(balance => balance.AccountName == "Girokonto").BalanceInCents);
        }

        [Fact]
        public async Task Reads_The_Balances_Of_All_Seven_Accounts_Of_The_Demo_Export()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();

            var balances = await ReadAsync(database);

            Assert.Equal((7, -11_716_574L), (balances.Count, balances.Sum(balance => balance.BalanceInCents)));
        }
    }
}
