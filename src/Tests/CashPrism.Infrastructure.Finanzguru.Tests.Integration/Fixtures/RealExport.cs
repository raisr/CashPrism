namespace CashPrism.Infrastructure.Finanzguru.Tests.Integration.Fixtures;

/// <summary>
/// A file Finanzguru actually wrote, anonymised with
/// <c>--synthetic-values</c> so it carries none of the real export's values.
/// Everything else — the header row, the sheet name, the inline strings, the
/// number formats, the styles — is what Finanzguru produced, which is the part
/// the workbooks built in code cannot vouch for. How it was produced, and how to
/// produce it again, is in <c>docs/finanzguru-export.md</c>.
/// </summary>
internal static class RealExport
{
    /// <summary>How many data rows the file carries: the <c>--max-rows</c> it was produced with.</summary>
    public const int DataRowCount = 50;

    private const string ResourceName = "20260907-Export-Alle_Buchungen-anonymised.xlsx";

    /// <summary>Opens the file from the test assembly. The caller disposes the stream.</summary>
    public static Stream Open()
        => typeof(RealExport).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The test assembly does not embed {ResourceName}.");
}
