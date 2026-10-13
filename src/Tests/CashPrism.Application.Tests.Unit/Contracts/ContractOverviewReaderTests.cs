using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using CashPrism.TestSupport.Contracts;

namespace CashPrism.Application.Tests.Unit.Contracts;

public sealed class ContractOverviewReaderTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 13);
    private static readonly DateTime Recently = new(2026, 10, 1);
    private static readonly DateTime LongAgo = new(2025, 1, 1);

    private static Contract Make(
        string counterparty,
        long amountInCents,
        string interval = "monatlich",
        bool isTransfer = false,
        DateTime? newestBookedOn = null)
        => new(Guid.NewGuid().ToString("N"), interval, counterparty, "Sonstiges", "Sonstige Ausgaben", amountInCents, "EUR", newestBookedOn ?? Recently, isTransfer);

    private static async Task<ContractOverview> ReadOverAsync(params Contract[] contracts)
        => (await new ContractOverviewReader(new FakeContractReader(ExportedOn, contracts)).ReadAsync())!;

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            var overview = await new ContractOverviewReader(new FakeContractReader(currentTo: null)).ReadAsync();

            Assert.Null(overview);
        }

        [Fact]
        public async Task Groups_Spending_Contracts_By_Interval_Shortest_First()
        {
            var overview = await ReadOverAsync(
                Make("Versicherung", -12_000, "jaehrlich"),
                Make("Strom", -9_000, "monatlich"),
                Make("Wasser", -3_000, "vierteljaehrlich"));

            Assert.Equal(["monatlich", "vierteljaehrlich", "jaehrlich"], overview.ByInterval.Select(group => group.Interval));
        }

        [Fact]
        public async Task Puts_An_Unknown_Interval_After_The_Known_Ones()
        {
            var overview = await ReadOverAsync(
                Make("Zeitung", -500, "woechentlich"),
                Make("Versicherung", -12_000, "jaehrlich"));

            Assert.Equal(["jaehrlich", "woechentlich"], overview.ByInterval.Select(group => group.Interval));
        }

        [Fact]
        public async Task Lists_The_Largest_Amount_First_Within_A_Group()
        {
            var overview = await ReadOverAsync(Make("Klein", -1_000), Make("Gross", -9_000));

            Assert.Equal(["Gross", "Klein"], overview.ByInterval.Single().Contracts.Select(contract => contract.Counterparty));
        }

        [Fact]
        public async Task Sums_A_Group_Over_Its_Current_Amounts()
        {
            var overview = await ReadOverAsync(Make("Strom", -9_000), Make("Internet", -3_999));

            Assert.Equal((2, -12_999L), (overview.ByInterval.Single().Count, overview.ByInterval.Single().SumInCents));
        }

        [Fact]
        public async Task Spreads_Each_Spending_Contract_Over_A_Month_For_The_Monthly_Cost()
        {
            var overview = await ReadOverAsync(
                Make("Strom", -9_000, "monatlich"),
                Make("Versicherung", -12_000, "jaehrlich"));

            Assert.Equal(10_000, overview.MonthlyCostInCents);
        }

        [Fact]
        public async Task Leaves_Income_Transfers_Ended_And_Unknown_Intervals_Out_Of_The_Monthly_Cost()
        {
            var overview = await ReadOverAsync(
                Make("Strom", -9_000),
                Make("Gehalt", 300_000),
                Make("Sparplan", -50_000, isTransfer: true),
                Make("Altvertrag", -2_000, newestBookedOn: LongAgo),
                Make("Zeitung", -500, "woechentlich"));

            Assert.Equal(9_000, overview.MonthlyCostInCents);
        }

        [Fact]
        public async Task Puts_Income_In_Its_Own_Section()
        {
            var overview = await ReadOverAsync(Make("Gehalt", 300_000), Make("Strom", -9_000));

            Assert.Equal(["Gehalt"], overview.Income.Contracts.Select(contract => contract.Counterparty));
        }

        [Fact]
        public async Task Puts_An_Incoming_Transfer_With_The_Transfers_Not_The_Income()
        {
            var overview = await ReadOverAsync(Make("Tagesgeld", 50_000, isTransfer: true));

            Assert.Equal((0, 1), (overview.Income.Count, overview.Transfers.Count));
        }

        [Fact]
        public async Task Puts_A_Presumably_Ended_Contract_Only_In_Its_Own_Section()
        {
            var overview = await ReadOverAsync(Make("Altvertrag", -2_000, newestBookedOn: LongAgo));

            Assert.Equal((0, 1), (overview.ByInterval.Count, overview.PresumablyEnded.Count));
        }

        [Fact]
        public async Task Takes_Ended_Income_Out_Of_The_Income_Section()
        {
            var overview = await ReadOverAsync(Make("Altes Gehalt", 300_000, newestBookedOn: LongAgo));

            Assert.Equal((0, 1), (overview.Income.Count, overview.PresumablyEnded.Count));
        }
    }
}
