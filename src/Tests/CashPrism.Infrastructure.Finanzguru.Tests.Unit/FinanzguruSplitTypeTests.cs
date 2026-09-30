using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;

namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruSplitTypeTests
{
    public sealed class Parse
    {
        [Theory]
        [InlineData("Original", SplitRole.Original)]
        [InlineData("Teilbuchung", SplitRole.Part)]
        [InlineData("Restbetrag", SplitRole.Remainder)]
        public void Translates_The_Word_The_Export_Writes(string value, SplitRole expected)
            => Assert.Equal(expected, FinanzguruSplitType.Parse(value, row: 2).Value);

        [Fact]
        public void Reads_An_Empty_Cell_As_The_Ordinary_Case()
            => Assert.Equal(SplitRole.None, FinanzguruSplitType.Parse(string.Empty, row: 2).Value);

        [Fact]
        public void Reads_A_Missing_Cell_As_The_Ordinary_Case()
            => Assert.Equal(SplitRole.None, FinanzguruSplitType.Parse(value: null, row: 2).Value);

        [Fact]
        public void Ignores_Surrounding_Whitespace()
            => Assert.Equal(SplitRole.Part, FinanzguruSplitType.Parse("  Teilbuchung ", row: 2).Value);

        [Fact]
        public void Ignores_The_Case_The_Word_Is_Written_In()
            => Assert.Equal(SplitRole.Remainder, FinanzguruSplitType.Parse("restbetrag", row: 2).Value);

        [Fact]
        public void Fails_And_Names_The_Row_For_A_Word_It_Does_Not_Know()
        {
            var result = FinanzguruSplitType.Parse("Sammelbuchung", row: 17);

            Assert.False(result.IsSuccess);
            Assert.Equivalent(
                ImportError.UnknownSplitType(
                    FinanzguruColumns.SplitType,
                    17,
                    "Sammelbuchung",
                    [FinanzguruSplitType.Original, FinanzguruSplitType.Part, FinanzguruSplitType.Remainder]),
                result.Error,
                strict: true);
        }
    }
}
