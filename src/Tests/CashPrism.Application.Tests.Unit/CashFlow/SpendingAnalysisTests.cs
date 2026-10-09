using CashPrism.Application.CashFlow;

namespace CashPrism.Application.Tests.Unit.CashFlow;

public sealed class SpendingAnalysisTests
{
    private static readonly DateOnly CurrentTo = new(2026, 10, 1);
    private static readonly DateOnly FirstMonth = new(2024, 10, 1);

    private static SpendingAnalysis Analyse(params CategoryHistory[] categories)
        => new(
            CurrentTo,
            Enumerable.Range(0, SpendingAnalysisReader.MonthsRead).Select(FirstMonth.AddMonths).ToList(),
            categories);

    /// <summary>
    /// A history of twenty-four months: <paramref name="latest"/> fills the
    /// last months, oldest first, and every month before them is
    /// <paramref name="earlier"/>.
    /// </summary>
    private static CategoryHistory History(string category, long earlier, params long[] latest)
        => new(
            category,
            [.. Enumerable.Repeat(earlier, SpendingAnalysisReader.MonthsRead - latest.Length), .. latest]);

    public sealed class Compare
    {
        [Fact]
        public void Takes_The_Latest_Months()
        {
            var period = Analyse().Compare(3);

            Assert.Equal([new(2026, 7, 1), new(2026, 8, 1), new(2026, 9, 1)], period.Months);
        }

        [Fact]
        public void Gives_Each_Category_Its_Spending_Of_The_Period()
        {
            var period = Analyse(History("Wohnen", 0, 100, 200, 300)).Compare(3);

            Assert.Equal([100L, 200L, 300L], period.Categories.Single().MonthlySpendingInCents);
        }

        [Fact]
        public void Averages_A_Category_Over_Every_Month_Of_The_Period_Rounded_To_The_Cent()
        {
            var period = Analyse(History("Wohnen", 0, 0, 100, 101)).Compare(3);

            Assert.Equal(67, period.Categories.Single().AverageInCents);
        }

        [Fact]
        public void Averages_All_Categories_Together()
        {
            var period = Analyse(
                History("Wohnen", 0, 300, 300, 300),
                History("Drogerie", 0, 0, 0, 30)).Compare(3);

            Assert.Equal(310, period.AverageInCents);
        }

        [Fact]
        public void Gives_Each_Category_Its_Share_Of_Everything_Spent()
        {
            var period = Analyse(
                History("Wohnen", 0, 300, 300, 300),
                History("Drogerie", 0, 100, 100, 100)).Compare(3);

            Assert.Equal([0.75m, 0.25m], period.Categories.Select(category => category.Share));
        }

        [Fact]
        public void Compares_With_The_Period_Of_The_Same_Length_Before()
        {
            var period = Analyse(History("Wohnen", 0, 100, 100, 100, 150, 150, 150)).Compare(3);

            Assert.Equal(0.5m, period.Categories.Single().Change);
        }

        [Fact]
        public void Has_No_Change_When_The_Category_Cost_Nothing_In_The_Period_Before()
        {
            var period = Analyse(History("Wohnen", 0, 150, 150, 150)).Compare(3);

            Assert.Null(period.Categories.Single().Change);
        }

        [Fact]
        public void Keeps_A_Category_That_Cost_Something_Only_In_The_Period_Before()
        {
            var period = Analyse(History("Wohnen", 0, 100, 0, 0, 0)).Compare(3);

            Assert.Equal(-1m, period.Categories.Single().Change);
        }

        [Fact]
        public void Leaves_Out_A_Category_That_Cost_Nothing_In_Either_Period()
        {
            var period = Analyse(History("Wohnen", 100, 0, 0, 0, 0, 0, 0)).Compare(3);

            Assert.Empty(period.Categories);
        }

        [Fact]
        public void Lists_The_Largest_Average_First()
        {
            var period = Analyse(
                History("Drogerie", 0, 10),
                History("Wohnen", 0, 500)).Compare(3);

            Assert.Equal(["Wohnen", "Drogerie"], period.Categories.Select(category => category.Category));
        }

        [Fact]
        public void Keeps_The_Rank_The_Category_Has_In_The_Analysis()
        {
            var period = Analyse(
                History("Drogerie", 0, 10),
                History("Wohnen", 0, 500)).Compare(3);

            Assert.Equal([1, 0], period.Categories.Select(category => category.Rank));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void Refuses_A_Period_Outside_What_Was_Read(int months)
            => Assert.Throws<ArgumentOutOfRangeException>(() => Analyse().Compare(months));
    }
}
