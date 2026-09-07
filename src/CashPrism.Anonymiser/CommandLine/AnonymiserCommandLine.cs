namespace CashPrism.Anonymiser.CommandLine;

/// <summary>
/// The command-line surface of the tool:
/// <c>CashPrism.Anonymiser &lt;input.xlsx&gt; [&lt;input2.xlsx&gt; …] --out &lt;directory&gt; [--force]</c>.
/// Hand-rolled, like <c>Shell/Hosting/HostingCommandLine</c> — four options do
/// not justify a command-line parsing library.
/// </summary>
public static class AnonymiserCommandLine
{
    /// <summary>The flag that makes an existing output file overwritable.</summary>
    public const string ForceSwitch = "--force";

    /// <summary>The switch that names the mandatory output directory.</summary>
    public const string OutSwitch = "--out";

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

        return AnonymiserCommandLineResult.Success(new AnonymiserOptions(inputFiles, outputDirectory, force));
    }
}
