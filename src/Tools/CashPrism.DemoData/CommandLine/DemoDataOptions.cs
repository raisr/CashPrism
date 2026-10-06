namespace CashPrism.DemoData.CommandLine;

/// <summary>
/// The parsed command-line arguments of one run: where to write the demo export
/// and which day it ends on.
/// </summary>
/// <param name="OutputDirectory">The directory the export is written into. Mandatory.</param>
/// <param name="Until">
/// The last day the export covers, which is also its export date. Defaults to
/// today; the same value always produces the same cells.
/// </param>
public sealed record DemoDataOptions(string OutputDirectory, DateOnly Until);
