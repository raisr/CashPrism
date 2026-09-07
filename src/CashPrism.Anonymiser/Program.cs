using CashPrism.Anonymiser.CommandLine;
using CashPrism.Anonymiser.Xlsx;

namespace CashPrism.Anonymiser;

/// <summary>
/// The composition root of the anonymiser: a development tool that turns a
/// real FinanzGuru export into one safe to share, without changing a single
/// value — see <c>Agents.md</c> for why this is its own console executable
/// rather than a feature of the running application.
/// </summary>
public sealed class Program
{
    private Program()
    {
    }

    /// <summary>
    /// Parses the command line and round-trips every input file in order.
    /// Stops at the first failure and returns a non-zero exit code; every
    /// failure prints one readable line to stderr, never a stack trace.
    /// </summary>
    public static int Main(string[] args)
    {
        var commandLine = AnonymiserCommandLine.Parse(args);

        if (!commandLine.IsSuccess)
        {
            Console.Error.WriteLine(commandLine.ErrorMessage);

            return 1;
        }

        var options = commandLine.Options!;

        foreach (var inputFile in options.InputFiles)
        {
            var result = XlsxRoundTrip.Run(inputFile, options.OutputDirectory, options.Force);

            if (!result.IsSuccess)
            {
                Console.Error.WriteLine(result.ErrorMessage);

                return 1;
            }

            Console.WriteLine(
                $"{inputFile} -> {result.OutputPath}: {result.RowsRead} rows read, {result.RowsWritten} rows written.");
        }

        return 0;
    }
}
