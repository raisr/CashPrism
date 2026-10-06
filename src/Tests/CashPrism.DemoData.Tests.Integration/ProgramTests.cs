using CashPrism.DemoData.Xlsx;
using ClosedXML.Excel;

namespace CashPrism.DemoData.Tests.Integration;

public sealed class ProgramTests
{
    public sealed class Main : IDisposable
    {
        private readonly string _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "cashprism-demodata-tests", Guid.NewGuid().ToString("N"))).FullName;

        public void Dispose() => Directory.Delete(_root, recursive: true);

        [Fact]
        public void Writes_One_Workbook_Named_After_The_Until_Date()
        {
            var exitCode = CashPrism.DemoData.Program.Main(["--out", _root, "--until", "2026-10-01"]);

            Assert.Equal(0, exitCode);
            var file = Assert.Single(Directory.GetFiles(_root));
            using var workbook = new XLWorkbook(file);
            Assert.Equal("20261001_Export_Alle_Buchungen", Assert.Single(workbook.Worksheets).Name);
        }

        [Fact]
        public void Creates_The_Out_Directory_Where_It_Is_Missing()
        {
            var outputDirectory = Path.Combine(_root, "new", "samples");

            CashPrism.DemoData.Program.Main(["--out", outputDirectory, "--until", "2026-10-01"]);

            Assert.True(File.Exists(Path.Combine(outputDirectory, DemoExportWriter.FileName)));
        }

        [Fact]
        public void Omitting_Out_Aborts_With_A_Non_Zero_Exit_Code()
            => Assert.NotEqual(0, CashPrism.DemoData.Program.Main(["--until", "2026-10-01"]));
    }
}
