using CashPrism.Application.Contracts;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Contracts;
using CashPrism.Domain.Imports;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Bookings;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class DismissedOverdueStoreTests
{
    private const string AContract = "0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f";
    private const string AnotherContract = "1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d";

    private static async Task StoreAsync(ThrowawayDatabase database, string contractId, DateOnly expectedOn)
    {
        await using var context = database.CreateContext();

        await new DismissedOverdueStore(context).DismissAsync(new DismissedOverdue(contractId, expectedOn));
    }

    private static async Task<IReadOnlyList<(string, DateOnly)>> ReadBackAsync(ThrowawayDatabase database)
    {
        await using var context = database.CreateContext();
        var dismissals = await new DismissedOverdueStore(context).ReadAllAsync();

        return [.. dismissals
            .Select(dismissal => (dismissal.ContractId, dismissal.ExpectedOn))
            .OrderBy(dismissal => dismissal.ContractId, StringComparer.Ordinal)];
    }

    public sealed class ReadAllAsync
    {
        [Fact]
        public async Task Reads_Nothing_From_A_Fresh_Database()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            Assert.Empty(await ReadBackAsync(database));
        }
    }

    public sealed class DismissAsync
    {
        [Fact]
        public async Task Stores_The_Contract_With_Its_Expected_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await StoreAsync(database, AContract, new DateOnly(2026, 10, 1));

            Assert.Equal([(AContract, new DateOnly(2026, 10, 1))], await ReadBackAsync(database));
        }

        [Fact]
        public async Task Replaces_An_Earlier_Date_Of_The_Same_Contract()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, AContract, new DateOnly(2026, 9, 1));

            await StoreAsync(database, AContract, new DateOnly(2026, 10, 1));

            Assert.Equal([(AContract, new DateOnly(2026, 10, 1))], await ReadBackAsync(database));
        }

        [Fact]
        public async Task Keeps_The_Dismissals_Of_Other_Contracts()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, AContract, new DateOnly(2026, 9, 1));

            await StoreAsync(database, AnotherContract, new DateOnly(2026, 10, 1));

            Assert.Equal(
                [(AContract, new DateOnly(2026, 9, 1)), (AnotherContract, new DateOnly(2026, 10, 1))],
                await ReadBackAsync(database));
        }

        [Fact]
        public async Task Accepts_The_Same_Dismissal_Twice()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, AContract, new DateOnly(2026, 10, 1));

            await StoreAsync(database, AContract, new DateOnly(2026, 10, 1));

            Assert.Equal([(AContract, new DateOnly(2026, 10, 1))], await ReadBackAsync(database));
        }
    }

    /// <summary>
    /// The dismissal as the contracts page sees it: through the real readers,
    /// over bookings and import runs in the same database.
    /// </summary>
    public sealed class WithTheContractForecasts
    {
        private static async Task ImportAsync(ThrowawayDatabase database, DateOnly exportedOn, params DateTime[] contractBookings)
        {
            await using var context = database.CreateContext();

            context.ImportRuns.Add(new ImportRun(
                Guid.NewGuid(),
                fileName: "buchungen.xlsx",
                fileHash: $"hash{exportedOn:yyyyMMdd}",
                sheetName: "Tabelle1",
                exportedOn,
                new DateTimeOffset(exportedOn.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)));
            context.Bookings.AddRange(contractBookings.Select(bookedOn => TestBookings.Create(
                Guid.NewGuid().ToString("N").PadLeft(40, '0'),
                bookedOn,
                -9_000,
                isContract: true,
                contractInterval: "monatlich",
                contractId: AContract)));
            await context.SaveChangesAsync();
        }

        private static async Task DismissAsync(ThrowawayDatabase database, DateOnly expectedOn)
        {
            await using var context = database.CreateContext();

            await new OverdueDismisser(new DismissedOverdueStore(context)).DismissAsync(AContract, expectedOn);
        }

        private static async Task<ContractForecast> ReadAsync(ThrowawayDatabase database)
        {
            await using var context = database.CreateContext();
            var forecasts = await new ContractForecastReader(new ContractReader(context), new DismissedOverdueStore(context)).ReadAsync();

            return forecasts!.Contracts.Single();
        }

        [Fact]
        public async Task A_Dismissed_Marker_Stays_Hidden_For_Its_Expected_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database, new DateOnly(2026, 10, 11), new DateTime(2026, 9, 1));

            await DismissAsync(database, new DateOnly(2026, 10, 1));

            Assert.Null((await ReadAsync(database)).DaysOverdue);
        }

        [Fact]
        public async Task A_Dismissed_Marker_Returns_For_The_Next_Expected_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await ImportAsync(database, new DateOnly(2026, 10, 11), new DateTime(2026, 9, 1));
            await DismissAsync(database, new DateOnly(2026, 10, 1));

            await ImportAsync(database, new DateOnly(2026, 11, 11), new DateTime(2026, 10, 1));

            var forecast = await ReadAsync(database);
            Assert.Equal((new DateOnly(2026, 11, 1), 10), (forecast.NextBookingOn, forecast.DaysOverdue));
        }
    }
}
