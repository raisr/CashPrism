namespace CashPrism.TestSupport.Samples;

/// <summary>
/// The demo export committed as <c>samples/demo-export.xlsx</c>: the file a
/// person without an export of their own tries CashPrism on. How it is made,
/// and how to make it again, is in <c>docs/demo-data.md</c>.
/// </summary>
public static class DemoSample
{
    /// <summary>The name the file is committed under.</summary>
    public const string FileName = "demo-export.xlsx";

    /// <summary>Opens the file from this assembly. The caller disposes the stream.</summary>
    public static Stream Open()
        => typeof(DemoSample).Assembly.GetManifestResourceStream(FileName)
            ?? throw new InvalidOperationException($"The test support assembly does not embed {FileName}.");
}
