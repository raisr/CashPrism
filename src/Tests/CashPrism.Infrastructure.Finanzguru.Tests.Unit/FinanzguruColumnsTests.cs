namespace CashPrism.Infrastructure.Finanzguru.Tests.Unit;

public sealed class FinanzguruColumnsTests
{
    public sealed class All
    {
        [Fact]
        public void Lists_The_Twentynine_Columns_Of_The_Measured_Export()
        {
            Assert.Equal(29, FinanzguruColumns.All.Count);
        }

        [Fact]
        public void Carries_No_Column_Twice()
        {
            Assert.Equal(
                FinanzguruColumns.All.Count,
                FinanzguruColumns.All.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void Carries_No_Blank_Or_Untrimmed_Name()
        {
            Assert.All(FinanzguruColumns.All, column =>
            {
                Assert.False(string.IsNullOrWhiteSpace(column));
                Assert.Equal(column.Trim(), column);
            });
        }
    }
}
