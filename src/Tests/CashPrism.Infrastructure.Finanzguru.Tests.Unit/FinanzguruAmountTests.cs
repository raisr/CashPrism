namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruAmountTests
{
    private const string Column = FinanzguruColumns.Amount;

    public sealed class ToCents
    {
        [Theory]
        [InlineData(0d, 0L)]
        [InlineData(1d, 100L)]
        [InlineData(12.34d, 1234L)]
        [InlineData(-12.34d, -1234L)]
        [InlineData(0.01d, 1L)]
        [InlineData(-0.01d, -1L)]
        public void Converts_A_Two_Decimal_Amount(double value, long expected)
        {
            var result = FinanzguruAmount.ToCents(value, Column, row: 2);

            Assert.True(result.IsSuccess);
            Assert.Equal(expected, result.AmountInCents);
        }

        /// <summary>
        /// The nearest double to 1234.56 is 1234.5599999999999, and multiplying
        /// that by 100 lands beside the whole cent. Going through decimal is
        /// what makes the cell read as the amount the spreadsheet shows.
        /// </summary>
        [Fact]
        public void Converts_An_Amount_A_Binary_Float_Cannot_Hold_Exactly()
        {
            var result = FinanzguruAmount.ToCents(1234.56d, Column, row: 2);

            Assert.True(result.IsSuccess);
            Assert.Equal(123456L, result.AmountInCents);
        }

        [Fact]
        public void Fails_When_The_Amount_Has_A_Third_Decimal_Place()
        {
            var result = FinanzguruAmount.ToCents(12.345d, Column, row: 7);

            Assert.False(result.IsSuccess);
            Assert.Contains("12.345", result.Error, StringComparison.Ordinal);
        }

        [Fact]
        public void Names_The_Column_And_The_Row_When_It_Fails()
        {
            var result = FinanzguruAmount.ToCents(12.345d, FinanzguruColumns.Balance, row: 7);

            Assert.Contains(FinanzguruColumns.Balance, result.Error, StringComparison.Ordinal);
            Assert.Contains("row 7", result.Error, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Fails_When_The_Value_Is_Not_A_Number(double value)
        {
            var result = FinanzguruAmount.ToCents(value, Column, row: 2);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Fails_When_The_Value_Is_Too_Large_For_A_Decimal()
        {
            var result = FinanzguruAmount.ToCents(1e30d, Column, row: 2);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Rejects_A_Missing_Column_Name()
            => Assert.Throws<ArgumentException>(() => FinanzguruAmount.ToCents(1d, " ", row: 2));
    }
}
