using CashPrism.Application.Contracts;
using CashPrism.Domain.Bookings;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Bookings;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The contracts behind the contracts page, against a real SQLite file. How
/// they are sorted into sections is the use case's business and tested there;
/// what only a database shows is that the newest booking of each contract is
/// the one read.
/// </summary>
public sealed class ContractReaderTests
{
    private const string AContract = "0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f";

    private static Booking CreateBooking(int ordinal, DateTime bookedOn, long amountInCents, string contractId = AContract)
        => TestBookings.Create(
            ordinal.ToString("D40", null),
            bookedOn,
            amountInCents,
            counterparty: $"Stadtwerke {ordinal}",
            isContract: contractId.Length > 0,
            contractInterval: contractId.Length > 0 ? "monatlich" : "",
            contractId: contractId);

    private static async Task<IReadOnlyList<Domain.Contracts.Contract>> ReadAsync(ThrowawayDatabase database, params Booking[] bookings)
    {
        await using (var seed = database.CreateContext())
        {
            seed.Bookings.AddRange(bookings);
            await seed.SaveChangesAsync();
        }

        await using var context = database.CreateContext();

        return await new ContractReader(context).ReadAllAsync();
    }

    public sealed class ReadAllAsync
    {
        [Fact]
        public async Task Describes_A_Contract_By_Its_Newest_Booking()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            var contracts = await ReadAsync(
                database,
                CreateBooking(1, new DateTime(2026, 9, 1), -8_000),
                CreateBooking(2, new DateTime(2026, 10, 1), -9_000),
                CreateBooking(3, new DateTime(2026, 8, 1), -7_000));

            Assert.Equal(("Stadtwerke 2", -9_000L), (contracts.Single().Counterparty, contracts.Single().AmountInCents));
        }

        [Fact]
        public async Task Leaves_Bookings_Without_A_Contract_Out()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            var contracts = await ReadAsync(database, CreateBooking(1, new DateTime(2026, 10, 1), -500, contractId: ""));

            Assert.Empty(contracts);
        }
    }

    public sealed class WithTheDemoExport
    {
        // Measured from samples/demo-export.xlsx outside CashPrism: 19
        // contracts as of 2026-10-01, the newest booking of each describing it.
        [Fact]
        public async Task Groups_The_Monthly_Contracts_With_Their_Count_And_Sum()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();
            await using var context = database.CreateContext();

            var overview = await new ContractOverviewReader(new ContractForecastReader(new ContractReader(context), new DismissedOverdueStore(context))).ReadAsync();
            var monthly = overview?.ByInterval.Single(group => group.Interval == "monatlich");

            Assert.Equal((9, -77_277L), (monthly?.Count, monthly?.SumInCents));
        }

        [Fact]
        public async Task Costs_The_Running_Spending_Contracts_Spread_Over_A_Month()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();
            await using var context = database.CreateContext();

            var overview = await new ContractOverviewReader(new ContractForecastReader(new ContractReader(context), new DismissedOverdueStore(context))).ReadAsync();

            Assert.Equal(87_991, overview?.MonthlyCostInCents);
        }

        [Fact]
        public async Task Finds_The_One_Presumably_Ended_Contract()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();
            await using var context = database.CreateContext();

            var overview = await new ContractOverviewReader(new ContractForecastReader(new ContractReader(context), new DismissedOverdueStore(context))).ReadAsync();

            Assert.Equal(["Flimmerkiste Streaming"], overview?.PresumablyEnded.Contracts.Select(forecast => forecast.Contract.Counterparty));
        }
    }
}
