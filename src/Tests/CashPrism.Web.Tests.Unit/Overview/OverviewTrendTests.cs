using CashPrism.Web.Components;
using CashPrism.Web.Overview;

namespace CashPrism.Web.Tests.Unit.Overview;

public sealed class OverviewTrendTests
{
    public sealed class OfChange
    {
        [Fact]
        public void Is_Up_For_A_Rise()
            => Assert.Equal(StatTrend.Up, OverviewTrend.OfChange(0.02m));

        [Fact]
        public void Is_Down_For_A_Fall()
            => Assert.Equal(StatTrend.Down, OverviewTrend.OfChange(-0.25m));

        [Fact]
        public void Is_Flat_For_A_Change_Shown_As_Zero_Per_Cent()
            => Assert.Equal(StatTrend.Flat, OverviewTrend.OfChange(0.004m));

        [Fact]
        public void Is_Unknown_Without_Anything_To_Compare_With()
            => Assert.Equal(StatTrend.Unknown, OverviewTrend.OfChange(null));
    }

    public sealed class OfDifference
    {
        [Fact]
        public void Is_Down_For_Less_Left()
            => Assert.Equal(StatTrend.Down, OverviewTrend.OfDifference(-58100));

        [Fact]
        public void Is_Flat_For_A_Difference_Shown_As_Zero_Euros()
            => Assert.Equal(StatTrend.Flat, OverviewTrend.OfDifference(40));
    }
}
