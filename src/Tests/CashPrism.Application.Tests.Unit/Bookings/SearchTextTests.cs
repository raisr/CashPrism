using CashPrism.Application.Bookings;

namespace CashPrism.Application.Tests.Unit.Bookings;

public sealed class SearchTextTests
{
    public sealed class Fold
    {
        [Fact]
        public void Writes_A_Text_In_Upper_Case()
            => Assert.Equal("REWE MARKT", SearchText.Fold("Rewe Markt"));

        [Theory]
        [InlineData("möbel")]
        [InlineData("MÖBEL")]
        [InlineData("moebel")]
        [InlineData("MOEBEL")]
        public void Spells_An_Umlaut_The_Way_The_Export_Does(string text)
            => Assert.Equal("MOEBEL", SearchText.Fold(text));

        [Theory]
        [InlineData("Straße")]
        [InlineData("STRAẞE")]
        [InlineData("strasse")]
        public void Spells_The_Sharp_S_As_Two(string text)
            => Assert.Equal("STRASSE", SearchText.Fold(text));

        [Theory]
        [InlineData("Café", "CAFE")]
        [InlineData("Niño", "NINO")]
        [InlineData("Garçon", "GARCON")]
        public void Leaves_Out_Every_Other_Accent(string text, string folded)
            => Assert.Equal(folded, SearchText.Fold(text));

        [Fact]
        public void Treats_An_Umlaut_Written_As_Two_Characters_Like_One()
            => Assert.Equal("MOEBEL", SearchText.Fold("möbel"));

        [Fact]
        public void Keeps_A_Letter_Unicode_Does_Not_Take_Apart()
            => Assert.Equal("ØRESUND", SearchText.Fold("Øresund"));

        [Fact]
        public void Keeps_Digits_And_Punctuation()
            => Assert.Equal("DE02 1203-0000", SearchText.Fold("de02 1203-0000"));
    }
}
