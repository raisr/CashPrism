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
    /// Parses the command line and anonymises every input file in one run.
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
        var result = XlsxAnonymiserRun.Run(options.InputFiles, options.OutputDirectory, options.Force);

        foreach (var fileResult in result.FileResults)
        {
            Console.WriteLine(
                $"{fileResult.InputPath} -> {fileResult.OutputPath}: {fileResult.RowsRead} rows read, {fileResult.RowsWritten} rows written.");
        }

        if (!result.IsSuccess)
        {
            Console.Error.WriteLine(result.ErrorMessage);

            return 1;
        }

        return 0;
    }
}
