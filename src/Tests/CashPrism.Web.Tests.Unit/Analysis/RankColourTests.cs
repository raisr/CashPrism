using CashPrism.Web.Analysis;

namespace CashPrism.Web.Tests.Unit.Analysis;

public sealed class RankColourTests
{
    public sealed class For
    {
        [Fact]
        public void Gives_The_Largest_Category_The_First_Colour_Of_The_Prism()
            => Assert.Equal("var(--cp-prism-violet)", RankColour.For(0));

        [Fact]
        public void Gives_Every_Category_With_A_Colour_Of_Its_Own_A_Different_One()
            => Assert.Equal(
                RankColour.Distinct,
                Enumerable.Range(0, RankColour.Distinct).Select(RankColour.For).Distinct().Count());

        [Fact]
        public void Gives_A_Category_Ranked_After_Them_The_Colour_Of_The_Rest()
            => Assert.Equal(RankColour.Rest, RankColour.For(RankColour.Distinct));

        [Fact]
        public void Refuses_A_Negative_Rank()
            => Assert.Throws<ArgumentOutOfRangeException>(() => RankColour.For(-1));
    }
}
