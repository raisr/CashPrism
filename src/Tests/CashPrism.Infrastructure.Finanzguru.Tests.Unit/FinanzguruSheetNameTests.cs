namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruSheetNameTests
{
    public sealed class TryParseExportDate
    {
        [Fact]
        public void Reads_The_Date_When_The_Name_Has_The_Export_Shape()
        {
            var parsed = FinanzguruSheetName.TryParseExportDate("20260907_Export_Alle_Buchungen", out var exportedOn);

            Assert.True(parsed);
            Assert.Equal(new DateOnly(2026, 9, 7), exportedOn);
        }

        [Fact]
        public void Ignores_Whitespace_Around_The_Name()
        {
            var parsed = FinanzguruSheetName.TryParseExportDate("  20260907_Export_Alle_Buchungen  ", out var exportedOn);

            Assert.True(parsed);
            Assert.Equal(new DateOnly(2026, 9, 7), exportedOn);
        }

        [Theory]
        [InlineData("20260907_Export_Alle_Umsaetze")]
        [InlineData("20260907")]
        [InlineData("Tabelle1")]
        public void Fails_When_The_Name_Does_Not_Carry_The_Export_Suffix(string sheetName)
        {
            var parsed = FinanzguruSheetName.TryParseExportDate(sheetName, out _);

            Assert.False(parsed);
        }

        [Theory]
        [InlineData("2026097_Export_Alle_Buchungen")]
        [InlineData("20261301_Export_Alle_Buchungen")]
        [InlineData("20260231_Export_Alle_Buchungen")]
        [InlineData("Export_Alle_Buchungen")]
        public void Fails_When_The_Leading_Part_Is_Not_A_Date(string sheetName)
        {
            var parsed = FinanzguruSheetName.TryParseExportDate(sheetName, out _);

            Assert.False(parsed);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Fails_When_The_Name_Is_Missing(string? sheetName)
        {
            var parsed = FinanzguruSheetName.TryParseExportDate(sheetName, out _);

            Assert.False(parsed);
        }

        [Fact]
        public void Leaves_The_Date_At_Its_Default_When_It_Fails()
        {
            FinanzguruSheetName.TryParseExportDate("Tabelle1", out var exportedOn);

            Assert.Equal(default, exportedOn);
        }
    }
}
