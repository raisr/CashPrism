using CashPrism.Infrastructure.Finanzguru.Tests.Integration.Fixtures;
using ClosedXML.Excel;

namespace CashPrism.Infrastructure.Finanzguru.Tests.Integration;

public sealed class FinanzguruColumnsTests
{
    public sealed class All
    {
        /// <summary>
        /// The header row is read with ClosedXML directly rather than through the
        /// reader: the reader resolves columns through this very list, so it
        /// could not tell a renamed or reordered column from an expected one.
        /// </summary>
        [Fact]
        public void Matches_The_Header_Row_Of_A_Real_Export_In_Order()
        {
            using var file = RealExport.Open();
            using var workbook = new XLWorkbook(file);

            var headerRow = workbook.Worksheet(1).Row(1).CellsUsed().Select(cell => cell.GetString());

            Assert.Equal(FinanzguruColumns.All, headerRow);
        }
    }
}
