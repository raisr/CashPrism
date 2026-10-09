using CashPrism.Web.Overview;

namespace CashPrism.Web.Tests.Unit.Overview;

public sealed class OverviewFormatTests
{
    public sealed class SumParts
    {
        [Fact]
        public void Writes_A_Sum_Without_A_Plus()
            => Assert.Equal(("4.082", ",50 €"), OverviewFormat.SumParts(408250));

        [Fact]
        public void Writes_A_Negative_Sum_With_The_Real_Minus()
            => Assert.Equal(("−312", ",40 €"), OverviewFormat.SumParts(-31240));
    }

    public sealed class WholeEuros
    {
        [Fact]
        public void Rounds_Half_A_Euro_Up()
            => Assert.Equal("1.439 €", OverviewFormat.WholeEuros(143850));

        [Fact]
        public void Writes_A_Negative_Sum_With_The_Real_Minus()
            => Assert.Equal("−45 €", OverviewFormat.WholeEuros(-4500));
    }

    public sealed class SignedWholeEuros
    {
        [Fact]
        public void Puts_A_Plus_Before_A_Rise()
            => Assert.Equal("+312 €", OverviewFormat.SignedWholeEuros(31200));

        [Fact]
        public void Puts_No_Sign_Before_Nothing()
            => Assert.Equal("0 €", OverviewFormat.SignedWholeEuros(40));
    }

    public sealed class Percent
    {
        [Fact]
        public void Writes_A_Rise_In_Whole_Per_Cent_With_A_Plus()
            => Assert.Equal("+2 %", OverviewFormat.Percent(0.020625m));

        [Fact]
        public void Writes_A_Fall_With_The_Real_Minus()
            => Assert.Equal("−29 %", OverviewFormat.Percent(-0.29m));

        [Fact]
        public void Writes_No_Sign_When_Nothing_Changed()
            => Assert.Equal("0 %", OverviewFormat.Percent(0.001m));
    }

    public sealed class Share
    {
        [Fact]
        public void Writes_Whole_Per_Cent_Without_A_Sign()
            => Assert.Equal("45 %", OverviewFormat.Share(0.4475m));
    }

    public sealed class MonthShort
    {
        [Fact]
        public void Drops_The_Full_Stop_Of_The_Abbreviation()
            => Assert.Equal("Okt 26", OverviewFormat.MonthShort(new DateOnly(2026, 10, 1)));
    }

    public sealed class MonthLong
    {
        [Fact]
        public void Writes_The_Month_And_The_Year_Out()
            => Assert.Equal("September 2026", OverviewFormat.MonthLong(new DateOnly(2026, 9, 1)));
    }

    public sealed class MonthName
    {
        [Fact]
        public void Writes_The_Month_Alone()
            => Assert.Equal("September", OverviewFormat.MonthName(new DateOnly(2026, 9, 1)));
    }
}
