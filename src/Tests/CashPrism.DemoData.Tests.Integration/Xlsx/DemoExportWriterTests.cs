using CashPrism.DemoData.Generation;
using CashPrism.DemoData.Xlsx;
using CashPrism.Infrastructure.Finanzguru;
using ClosedXML.Excel;

namespace CashPrism.DemoData.Tests.Integration.Xlsx;

public sealed class DemoExportWriterTests
{
    public sealed class Write : IDisposable
    {
        private static readonly DateOnly Until = new(2026, 10, 1);

        private readonly string _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "cashprism-demodata-tests", Guid.NewGuid().ToString("N"))).FullName;

        public void Dispose() => Directory.Delete(_root, recursive: true);

        [Fact]
        public void Writes_The_Same_Cells_Twice_For_The_Same_Day()
        {
            var first = WriteExport("first");
            var second = WriteExport("second");

            Assert.Equal(WorkbookContent.Read(first), WorkbookContent.Read(second));
        }

        [Fact]
        public void Writes_A_File_The_Export_Reader_Reads_Without_An_Error()
        {
            using var stream = File.OpenRead(WriteExport());

            var result = new FinanzguruExportReader().Read(stream);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Errors));
            Assert.Empty(result.Export!.UnknownColumns);
            Assert.Equal(DemoExport.Generate(Until).Count, result.Export.Rows.Count);
        }

        [Fact]
        public void Writes_Every_Known_Column_In_Export_Order()
        {
            using var workbook = new XLWorkbook(WriteExport());

            var header = workbook.Worksheet(1).Row(1).CellsUsed().Select(cell => cell.GetText());

            Assert.Equal(FinanzguruColumns.All, header);
        }

        [Fact]
        public void Leaves_E_Ref_Empty_In_Every_Row()
        {
            using var workbook = new XLWorkbook(WriteExport());
            var column = FinanzguruColumns.All.ToList().IndexOf(FinanzguruColumns.EndToEndReference) + 1;

            Assert.Single(workbook.Worksheet(1).Column(column).CellsUsed());
        }

        [Fact]
        public void Formats_The_Booking_Date_As_A_Day_And_The_Money_As_Two_Decimals()
        {
            using var workbook = new XLWorkbook(WriteExport());
            var row = workbook.Worksheet(1).Row(2);

            Assert.Equal("dd.MM.yyyy", row.Cell(1).Style.NumberFormat.Format);
            Assert.Equal(4, row.Cell(4).Style.NumberFormat.NumberFormatId);
            Assert.Equal(4, row.Cell(5).Style.NumberFormat.NumberFormatId);
        }

        [Fact]
        public void Writes_The_Year_As_A_Number()
        {
            using var workbook = new XLWorkbook(WriteExport());
            var column = FinanzguruColumns.All.ToList().IndexOf(FinanzguruColumns.Year) + 1;

            Assert.Equal(XLDataType.Number, workbook.Worksheet(1).Cell(2, column).DataType);
        }

        private string WriteExport(string directory = "out")
            => DemoExportWriter.Write(DemoExport.Generate(Until), Until, Path.Combine(_root, directory));
    }
}
