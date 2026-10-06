namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruPeriodLabelsTests
{
    public sealed class Week
    {
        // 1 January 2026 is a Thursday, so week 1 is the three days up to the
        // first Saturday and week 2 starts on Sunday, 4 January.
        [Theory]
        [InlineData(2026, 1, 1, "2026-01")]
        [InlineData(2026, 1, 3, "2026-01")]
        [InlineData(2026, 1, 4, "2026-02")]
        [InlineData(2026, 1, 10, "2026-02")]
        [InlineData(2026, 1, 11, "2026-03")]
        [InlineData(2026, 12, 31, "2026-53")]
        public void Counts_Weeks_From_Sunday_With_Week_One_Starting_On_New_Year(
            int year, int month, int day, string expected)
            => Assert.Equal(expected, FinanzguruPeriodLabels.Week(new DateOnly(year, month, day)));
    }

    public sealed class Month
    {
        [Fact]
        public void Writes_Year_And_Two_Digit_Month()
            => Assert.Equal("2001-03", FinanzguruPeriodLabels.Month(new DateOnly(2001, 3, 15)));
    }

    public sealed class Quarter
    {
        [Theory]
        [InlineData(1, "2001-Q1")]
        [InlineData(3, "2001-Q1")]
        [InlineData(4, "2001-Q2")]
        [InlineData(12, "2001-Q4")]
        public void Writes_Year_And_Quarter(int month, string expected)
            => Assert.Equal(expected, FinanzguruPeriodLabels.Quarter(new DateOnly(2001, month, 1)));
    }

    public sealed class Year
    {
        [Fact]
        public void Writes_The_Year_The_Way_The_Export_Does()
            => Assert.Equal("2001.0", FinanzguruPeriodLabels.Year(new DateOnly(2001, 6, 1)));
    }
}
