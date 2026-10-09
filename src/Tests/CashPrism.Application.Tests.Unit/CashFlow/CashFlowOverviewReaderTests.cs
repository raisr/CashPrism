using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.CashFlow;

namespace CashPrism.Application.Tests.Unit.CashFlow;

public sealed class CashFlowOverviewReaderTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 1);
    private static readonly DateOnly October = new(2026, 10, 1);
    private static readonly DateOnly September = new(2026, 9, 1);
    private static readonly DateOnly August = new(2026, 8, 1);

    private static Task<CashFlowOverview?> ReadOverAsync(params CategoryNet[] nets)
        => new CashFlowOverviewReader(new FakeCashFlowReader(ExportedOn, nets)).ReadAsync();

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            var overview = await new CashFlowOverviewReader(new FakeCashFlowReader(currentTo: null)).ReadAsync();

            Assert.Null(overview);
        }

        [Fact]
        public async Task Reads_Twenty_Four_Months_Ending_With_The_Month_The_Export_Was_Taken_In()
        {
            var reader = new FakeCashFlowReader(ExportedOn);

            await new CashFlowOverviewReader(reader).ReadAsync();

            Assert.Equal((new DateOnly(2024, 11, 1), October), reader.MonthsAskedFor);
        }

        [Fact]
        public async Task Counts_The_Categories_That_Came_Out_Positive_As_Income_And_The_Others_As_Spending()
        {
            var overview = await ReadOverAsync(
                new CategoryNet(September, "Einnahmen", 841607),
                new CategoryNet(September, "Sparen", 377959),
                new CategoryNet(September, "Wohnen", -56800),
                new CategoryNet(September, "Drogerie", -2100));

            Assert.Equal(new MonthlyCashFlow(September, 1219566, 58900), overview?.LastCompleteMonth);
        }

        [Fact]
        public async Task Fills_A_Month_Without_Bookings_With_Zero()
        {
            var overview = await ReadOverAsync(new CategoryNet(September, "Einnahmen", 400000));

            Assert.Equal(new MonthlyCashFlow(August, 0, 0), overview?.MonthBefore);
        }

        [Fact]
        public async Task Takes_The_Month_Before_The_Running_One_As_The_Last_Complete_Month()
        {
            var overview = await ReadOverAsync(
                new CategoryNet(September, "Einnahmen", 400000),
                new CategoryNet(October, "Wohnen", -4500));

            Assert.Equal(new MonthlyCashFlow(September, 400000, 0), overview?.LastCompleteMonth);
        }

        [Fact]
        public async Task Lists_The_Categories_That_Cost_Money_In_The_Last_Complete_Month_Largest_First()
        {
            var overview = await ReadOverAsync(
                new CategoryNet(August, "Lifestyle", -99999),
                new CategoryNet(September, "Drogerie", -2100),
                new CategoryNet(September, "Einnahmen", 841607),
                new CategoryNet(September, "Wohnen", -56800),
                new CategoryNet(October, "Kinder", -44000));

            Assert.Equal(
                [new CategorySpending("Wohnen", 56800), new CategorySpending("Drogerie", 2100)],
                overview?.SpendingByCategory);
        }
    }
}
