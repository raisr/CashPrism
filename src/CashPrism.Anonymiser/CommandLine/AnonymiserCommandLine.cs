using System.Globalization;

namespace CashPrism.Anonymiser.CommandLine;

/// <summary>
/// The command-line surface of the tool:
/// <c>CashPrism.Anonymiser &lt;input.xlsx&gt; [&lt;input2.xlsx&gt; …] --out &lt;directory&gt;
/// [--force] [--scale &lt;factor&gt;] [--max-rows &lt;n&gt;]</c>.
/// Hand-rolled, like <c>Shell/Hosting/HostingCommandLine</c> — a handful of
/// options do not justify a command-line parsing library.
/// </summary>
public static class AnonymiserCommandLine
{
    /// <summary>The flag that makes an existing output file overwritable.</summary>
    public const string ForceSwitch = "--force";

    /// <summary>The switch that names the mandatory output directory.</summary>
    public const string OutSwitch = "--out";

    /// <summary>The switch that scales <c>Betrag</c> and <c>Kontostand</c> together.</summary>
    public const string ScaleSwitch = "--scale";

    /// <summary>The switch that keeps only the newest <c>n</c> data rows.</summary>
    public const string MaxRowsSwitch = "--max-rows";

    /// <summary>
    /// Parses <paramref name="args"/> into <see cref="AnonymiserOptions"/>, or
    /// reports the one reason it could not.
    /// </summary>
    public static AnonymiserCommandLineResult Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var inputFiles = new List<string>();
        string? outputDirectory = null;
        var force = false;
        var scale = 1.0m;
        int? maxRows = null;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];

            switch (argument)
            {
                case OutSwitch:
                    if (index + 1 >= args.Count)
                    {
                        return AnonymiserCommandLineResult.Failure(
                            $"{OutSwitch} requires a directory.");
                    }

                    outputDirectory = args[++index];
                    break;

                case ForceSwitch:
                    force = true;
                    break;

                case ScaleSwitch:
                    if (index + 1 >= args.Count)
                    {
                        return AnonymiserCommandLineResult.Failure($"{ScaleSwitch} requires a factor.");
                    }

                    var scaleText = args[++index];

                    if (!decimal.TryParse(scaleText, NumberStyles.Float, CultureInfo.InvariantCulture, out scale)
                        || scale <= 0)
                    {
                        return AnonymiserCommandLineResult.Failure(
                            $"{ScaleSwitch} requires a positive number, got '{scaleText}'.");
                    }

                    break;

                case MaxRowsSwitch:
                    if (index + 1 >= args.Count)
                    {
                        return AnonymiserCommandLineResult.Failure($"{MaxRowsSwitch} requires a number.");
                    }

                    var maxRowsText = args[++index];

                    if (!int.TryParse(maxRowsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedMaxRows)
                        || parsedMaxRows < 0)
                    {
                        return AnonymiserCommandLineResult.Failure(
                            $"{MaxRowsSwitch} requires a non-negative whole number, got '{maxRowsText}'.");
                    }

                    maxRows = parsedMaxRows;
                    break;

                case ['-', '-', ..]:
                    return AnonymiserCommandLineResult.Failure($"Unknown option: {argument}");

                default:
                    inputFiles.Add(argument);
                    break;
            }
        }

        if (inputFiles.Count == 0)
        {
            return AnonymiserCommandLineResult.Failure("At least one input .xlsx file is required.");
        }

        if (outputDirectory is null)
        {
            return AnonymiserCommandLineResult.Failure($"{OutSwitch} <directory> is required.");
        }

        return AnonymiserCommandLineResult.Success(
            new AnonymiserOptions(inputFiles, outputDirectory, force, scale, maxRows));
    }
}
