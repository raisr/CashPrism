using CashPrism.DemoData.Generation;
using CashPrism.DemoData.Tests.Integration.Xlsx;
using CashPrism.DemoData.Xlsx;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.TestSupport.Samples;

namespace CashPrism.DemoData.Tests.Integration;

/// <summary>
/// The demo export committed as <c>samples/demo-export.xlsx</c>. It is what a
/// person downloads to try CashPrism, so it has to be what the generator makes
/// and what the reader reads — and a change to either that the committed file
/// did not follow fails here, naming the way back: regenerate it.
/// </summary>
public sealed class DemoSampleTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "cashprism-demodata-tests", Guid.NewGuid().ToString("N"))).FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Is_Read_By_The_Export_Reader_Without_An_Error_Or_An_Unknown_Column()
    {
        using var stream = DemoSample.Open();

        var result = new FinanzguruExportReader().Read(stream);

        Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Errors));
        Assert.Empty(result.Export!.UnknownColumns);
    }

    /// <summary>
    /// Compares cell content rather than bytes: the zip container records when
    /// it was written, so two generations of the same day never match byte for
    /// byte. When this fails, the generator changed — regenerate the file as
    /// <c>docs/demo-data.md</c> describes.
    /// </summary>
    [Fact]
    public void Matches_A_Fresh_Generation_For_Its_Own_Export_Date()
    {
        using var committed = DemoSample.Open();
        var exportedOn = ExportDate();

        var fresh = DemoExportWriter.Write(DemoExport.Generate(exportedOn), exportedOn, _root);

        Assert.Equal(WorkbookContent.Read(fresh), WorkbookContent.Read(committed));
    }

    private static DateOnly ExportDate()
    {
        using var stream = DemoSample.Open();

        return new FinanzguruExportReader().Read(stream).Export!.ExportedOn;
    }
}
