using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.CashFlow;

namespace CashPrism.Application.Tests.Unit.CashFlow;

public sealed class SpendingAnalysisReaderTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 1);
    private static readonly DateOnly September = new(2026, 9, 1);
    private static readonly DateOnly August = new(2026, 8, 1);
    private static readonly DateOnly September2025 = new(2025, 9, 1);

    private static Task<SpendingAnalysis?> ReadOverAsync(params CategoryNet[] nets)
        => new SpendingAnalysisReader(new FakeCashFlowReader(ExportedOn, nets)).ReadAsync();

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            var analysis = await new SpendingAnalysisReader(new FakeCashFlowReader(currentTo: null)).ReadAsync();

            Assert.Null(analysis);
        }

        [Fact]
        public async Task Reads_Twenty_Four_Complete_Months_Ending_Before_The_Month_The_Export_Was_Taken_In()
        {
            var reader = new FakeCashFlowReader(ExportedOn);

            await new SpendingAnalysisReader(reader).ReadAsync();

            Assert.Equal((new DateOnly(2024, 10, 1), September), reader.MonthsAskedFor);
        }

        [Fact]
        public async Task Lists_The_Months_Oldest_First()
        {
            var analysis = await ReadOverAsync();

            Assert.Equal((new DateOnly(2024, 10, 1), September), (analysis?.Months[0], analysis?.Months[^1]));
        }

        [Fact]
        public async Task Fills_A_Month_The_Category_Cost_Nothing_In_With_Zero()
        {
            var analysis = await ReadOverAsync(new CategoryNet(September, "Wohnen", -56800));

            Assert.Equal([.. Enumerable.Repeat(0L, 23), 56800L], analysis?.Categories.Single().MonthlySpendingInCents);
        }

        [Fact]
        public async Task Counts_A_Month_The_Category_Came_Out_Positive_In_As_Nothing_Spent()
        {
            var analysis = await ReadOverAsync(
                new CategoryNet(August, "Wohnen", 12000),
                new CategoryNet(September, "Wohnen", -56800));

            Assert.Equal(0, analysis?.Categories.Single().MonthlySpendingInCents[^2]);
        }

        [Fact]
        public async Task Leaves_Out_A_Category_That_Never_Cost_Anything()
        {
            var analysis = await ReadOverAsync(
                new CategoryNet(September, "Einnahmen", 841607),
                new CategoryNet(September, "Wohnen", -56800));

            Assert.Equal(["Wohnen"], analysis?.Categories.Select(category => category.Category));
        }

        [Fact]
        public async Task Ranks_The_Categories_By_What_They_Cost_In_The_Latest_Twelve_Months()
        {
            var analysis = await ReadOverAsync(
                new CategoryNet(September2025, "Lifestyle", -999999),
                new CategoryNet(August, "Drogerie", -2100),
                new CategoryNet(September, "Wohnen", -56800));

            Assert.Equal(
                ["Wohnen", "Drogerie", "Lifestyle"],
                analysis?.Categories.Select(category => category.Category));
        }
    }
}
