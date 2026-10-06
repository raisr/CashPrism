using CashPrism.DemoData.CommandLine;
using CashPrism.DemoData.Generation;
using CashPrism.DemoData.Xlsx;

namespace CashPrism.DemoData;

/// <summary>
/// The composition root of the demo data generator: a development tool that
/// writes a fully synthetic Finanzguru export, for trying CashPrism without
/// data of one's own. See <c>docs/demo-data.md</c>.
/// </summary>
public sealed class Program
{
    private Program()
    {
    }

    /// <summary>
    /// Parses the command line, generates the export and writes it. A command
    /// line that does not add up prints one readable line to stderr and returns
    /// a non-zero exit code.
    /// </summary>
    public static int Main(string[] args)
    {
        var today = DateOnly.FromDateTime(TimeProvider.System.GetLocalNow().DateTime);
        var commandLine = DemoDataCommandLine.Parse(args, today);

        if (!commandLine.IsSuccess)
        {
            Console.Error.WriteLine(commandLine.ErrorMessage);

            return 1;
        }

        var options = commandLine.Options!;
        var rows = DemoExport.Generate(options.Until);
        var path = DemoExportWriter.Write(rows, options.Until, options.OutputDirectory);

        Console.WriteLine($"{path}: {rows.Count} rows, sheet {DemoExport.SheetName(options.Until)}.");

        return 0;
    }
}
