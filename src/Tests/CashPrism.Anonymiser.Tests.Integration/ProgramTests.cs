using CashPrism.Anonymiser.Tests.Integration.Fixtures;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Integration;

public sealed class ProgramTests
{
    public sealed class Main : IDisposable
    {
        private readonly string _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "cashprism-anonymiser-tests", Guid.NewGuid().ToString("N"))).FullName;

        public void Dispose() => Directory.Delete(_root, recursive: true);

        [Fact]
        public void Writes_The_Anonymised_File_Into_The_Out_Directory()
        {
            var inputPath = Path.Combine(_root, "export.xlsx");
            File.WriteAllBytes(inputPath, XlsxTestWorkbook.Build(FinanzguruColumns.All, []));

            var outputDirectory = Path.Combine(_root, "out");

            var exitCode = CashPrism.Anonymiser.Program.Main([inputPath, "--out", outputDirectory]);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(Path.Combine(outputDirectory, "export-anonymised.xlsx")));
        }

        [Fact]
        public void Omitting_Out_Aborts_With_A_Non_Zero_Exit_Code()
        {
            var inputPath = Path.Combine(_root, "export.xlsx");
            File.WriteAllBytes(inputPath, XlsxTestWorkbook.Build(FinanzguruColumns.All, []));

            var exitCode = CashPrism.Anonymiser.Program.Main([inputPath]);

            Assert.NotEqual(0, exitCode);
        }
    }
}
