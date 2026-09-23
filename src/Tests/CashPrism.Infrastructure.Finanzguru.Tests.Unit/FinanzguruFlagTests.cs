namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruFlagTests
{
    public sealed class Parse
    {
        [Fact]
        public void Reads_The_German_Word_For_Yes_As_True()
        {
            var result = FinanzguruFlag.Parse("ja", FinanzguruColumns.IsInternalTransfer, row: 2);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public void Reads_The_German_Word_For_No_As_False()
        {
            var result = FinanzguruFlag.Parse("nein", FinanzguruColumns.IsInternalTransfer, row: 2);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }

        [Fact]
        public void Ignores_Surrounding_Whitespace()
        {
            var result = FinanzguruFlag.Parse("  ja  ", FinanzguruColumns.IsContract, row: 2);

            Assert.True(result.Value);
        }

        [Fact]
        public void Ignores_Case()
        {
            var result = FinanzguruFlag.Parse("Nein", FinanzguruColumns.IsContract, row: 2);

            Assert.False(result.Value);
        }

        [Fact]
        public void Refuses_A_Third_Word_Rather_Than_Defaulting_To_False()
        {
            var result = FinanzguruFlag.Parse("vielleicht", FinanzguruColumns.IsInternalTransfer, row: 4711);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Names_The_Column_And_The_Row_Of_A_Value_It_Cannot_Read()
        {
            var result = FinanzguruFlag.Parse("vielleicht", FinanzguruColumns.IsInternalTransfer, row: 4711);

            Assert.Contains(FinanzguruColumns.IsInternalTransfer, result.Error!, StringComparison.Ordinal);
            Assert.Contains("4711", result.Error!, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Refuses_An_Empty_Cell(string? value)
        {
            var result = FinanzguruFlag.Parse(value, FinanzguruColumns.ExcludedFromDisposableIncome, row: 2);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Refuses_A_Blank_Column_Name()
            => Assert.Throws<ArgumentException>(() => FinanzguruFlag.Parse("ja", column: " ", row: 2));
    }
}
