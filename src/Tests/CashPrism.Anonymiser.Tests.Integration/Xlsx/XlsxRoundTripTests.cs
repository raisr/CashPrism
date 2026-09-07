using System.IO.Compression;
using CashPrism.Anonymiser.Tests.Integration.Fixtures;
using CashPrism.Anonymiser.Xlsx;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Integration.Xlsx;

public sealed class XlsxRoundTripTests
{
    public sealed class Run : IDisposable
    {
        private readonly string _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "cashprism-anonymiser-tests", Guid.NewGuid().ToString("N"))).FullName;

        private readonly string _outputDirectory;

        public Run()
        {
            _outputDirectory = Path.Combine(_root, "out");
        }

        public void Dispose() => Directory.Delete(_root, recursive: true);

        [Fact]
        public void Every_Zip_Entry_Is_Byte_Identical_To_The_Input()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var result = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.RowsRead);
            Assert.Equal(2, result.RowsWritten);

            using var expected = ZipFile.OpenRead(inputPath);
            using var actual = ZipFile.OpenRead(result.OutputPath!);

            var expectedNames = expected.Entries.Select(e => e.FullName).OrderBy(n => n, StringComparer.Ordinal);
            var actualNames = actual.Entries.Select(e => e.FullName).OrderBy(n => n, StringComparer.Ordinal);

            Assert.Equal(expectedNames, actualNames);

            foreach (var entry in expected.Entries)
            {
                var actualEntry = actual.GetEntry(entry.FullName);

                Assert.NotNull(actualEntry);
                Assert.Equal(ReadAllBytes(entry), ReadAllBytes(actualEntry!));
            }
        }

        [Fact]
        public void Writes_The_Output_Next_To_The_Input_Name_With_Anonymised_Inserted()
        {
            var inputPath = WriteInput(BuildValidWorkbook(), fileName: "export.xlsx");

            var result = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);

            Assert.True(result.IsSuccess);
            Assert.Equal(Path.Combine(_outputDirectory, "export-anonymised.xlsx"), result.OutputPath);
            Assert.True(File.Exists(result.OutputPath));
        }

        [Fact]
        public void Missing_Known_Column_Aborts_Naming_It()
        {
            var headerNames = FinanzguruColumns.All.Where(c => c != FinanzguruColumns.Tags).ToArray();
            var inputPath = WriteInput(XlsxTestWorkbook.Build(headerNames, []));

            var result = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.Tags, result.ErrorMessage);
        }

        [Fact]
        public void Unknown_Extra_Column_Aborts_Naming_It()
        {
            string[] headerNames = [.. FinanzguruColumns.All, "Analyse-Sondertarif"];
            var inputPath = WriteInput(XlsxTestWorkbook.Build(headerNames, []));

            var result = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);

            Assert.False(result.IsSuccess);
            Assert.Contains("Analyse-Sondertarif", result.ErrorMessage);
        }

        [Fact]
        public void Shared_Strings_Abort_Naming_The_Reason()
        {
            var inputPath = WriteInput(
                XlsxTestWorkbook.Build(FinanzguruColumns.All, [], useSharedStrings: true));

            var result = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);

            Assert.False(result.IsSuccess);
            Assert.Contains("shared strings", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Existing_Output_Aborts_Without_Force()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var first = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);
            Assert.True(first.IsSuccess);

            var second = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);

            Assert.False(second.IsSuccess);
            Assert.Contains("already exists", second.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Existing_Output_Is_Overwritten_With_Force()
        {
            var inputPath = WriteInput(BuildValidWorkbook());

            var first = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: false);
            Assert.True(first.IsSuccess);

            var second = XlsxRoundTrip.Run(inputPath, _outputDirectory, force: true);

            Assert.True(second.IsSuccess);
        }

        private static byte[] BuildValidWorkbook()
        {
            static Dictionary<string, string> Row(string counterparty, string bookingDate) => new()
            {
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.BookingDate] = bookingDate,
            };

            return XlsxTestWorkbook.Build(
                FinanzguruColumns.All,
                [Row("Bakery", "01.03.2026"), Row("Landlord", "02.03.2026")]);
        }

        private string WriteInput(byte[] content, string fileName = "export.xlsx")
        {
            var path = Path.Combine(_root, fileName);
            File.WriteAllBytes(path, content);

            return path;
        }

        private static byte[] ReadAllBytes(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            using var buffer = new MemoryStream();

            stream.CopyTo(buffer);

            return buffer.ToArray();
        }
    }
}
