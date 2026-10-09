using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class ChartTipContentTests
{
    public sealed class AnchorAt
    {
        [Fact]
        public void Leans_The_First_Mark_Towards_The_End()
        {
            Assert.Equal(ChartTipAnchor.Start, ChartTipContent.AnchorAt(0, 12));
        }

        [Fact]
        public void Leans_The_Last_Mark_Towards_The_Start()
        {
            Assert.Equal(ChartTipAnchor.End, ChartTipContent.AnchorAt(11, 12));
        }

        [Fact]
        public void Centres_A_Mark_In_Between()
        {
            Assert.Equal(ChartTipAnchor.Centre, ChartTipContent.AnchorAt(5, 12));
        }
    }
}
