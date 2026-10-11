using CashPrism.Application.Bookings;

namespace CashPrism.Application.Tests.Unit.Bookings;

public sealed class BookingSearchTests
{
    public sealed class Parse
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Searches_For_Nothing_Without_A_Word(string? text)
            => Assert.Null(BookingSearch.Parse(text));

        [Fact]
        public void Reads_Free_Text_As_Words()
            => Assert.Equal(["rewe", "berlin"], BookingSearch.Parse("rewe  berlin")!.Words);

        [Fact]
        public void Keeps_The_Text_Without_Surrounding_Spaces()
            => Assert.Equal(">200 rewe", BookingSearch.Parse("  >200 rewe ")!.Text);

        [Theory]
        [InlineData(">200", AmountComparison.GreaterThan)]
        [InlineData(">=200", AmountComparison.GreaterThanOrEqual)]
        [InlineData("<200", AmountComparison.LessThan)]
        [InlineData("<=200", AmountComparison.LessThanOrEqual)]
        [InlineData("=200", AmountComparison.Equal)]
        public void Reads_Each_Operator_As_An_Amount_Bound(string text, AmountComparison comparison)
            => Assert.Equal([new AmountBound(comparison, 20000)], BookingSearch.Parse(text)!.AmountBounds);

        [Theory]
        [InlineData(">12,50")]
        [InlineData(">12.50")]
        public void Takes_A_Comma_Or_A_Point_Before_The_Cents(string text)
            => Assert.Equal([new AmountBound(AmountComparison.GreaterThan, 1250)], BookingSearch.Parse(text)!.AmountBounds);

        [Fact]
        public void Takes_A_Single_Decimal_As_Tenths()
            => Assert.Equal([new AmountBound(AmountComparison.LessThan, 450)], BookingSearch.Parse("<4,5")!.AmountBounds);

        [Theory]
        [InlineData(">200EUR")]
        [InlineData(">200eur")]
        [InlineData(">200€")]
        [InlineData(">200 €")]
        [InlineData(">200 EUR")]
        public void Ignores_A_Currency_Behind_The_Amount(string text)
        {
            var search = BookingSearch.Parse(text)!;

            Assert.Equal([new AmountBound(AmountComparison.GreaterThan, 20000)], search.AmountBounds);
            Assert.Empty(search.Words);
        }

        [Fact]
        public void Joins_Several_Bounds_And_Words()
        {
            var search = BookingSearch.Parse(">300 <400 rewe")!;

            Assert.Equal(
                [new AmountBound(AmountComparison.GreaterThan, 30000), new AmountBound(AmountComparison.LessThan, 40000)],
                search.AmountBounds);
            Assert.Equal(["rewe"], search.Words);
        }

        [Theory]
        [InlineData(">abc")]
        [InlineData(">")]
        [InlineData(">-5")]
        [InlineData(">1.000,00")]
        [InlineData(">12,345")]
        [InlineData(">,50")]
        public void Searches_For_A_Token_That_Is_No_Amount_As_A_Word(string text)
        {
            var search = BookingSearch.Parse(text)!;

            Assert.Empty(search.AmountBounds);
            Assert.Equal([text], search.Words);
        }

        [Fact]
        public void Searches_For_A_Currency_Without_An_Amount_Before_It_As_A_Word()
            => Assert.Equal(["€"], BookingSearch.Parse("€")!.Words);
    }

    public new sealed class Equals
    {
        [Fact]
        public void Takes_Two_Searches_With_The_Same_Text_As_The_Same()
            => Assert.Equal(BookingSearch.Parse(">200 rewe"), BookingSearch.Parse(">200 rewe"));

        [Fact]
        public void Tells_Two_Searches_With_Different_Text_Apart()
            => Assert.NotEqual(BookingSearch.Parse(">200 rewe"), BookingSearch.Parse("rewe >200"));
    }
}
