using System.Globalization;

namespace CashPrism.DemoData.CommandLine;

/// <summary>
/// The command-line surface of the tool:
/// <c>CashPrism.DemoData --out &lt;directory&gt; [--until &lt;yyyy-MM-dd&gt;]</c>.
/// Hand-rolled, like the anonymiser's — two options do not justify a
/// command-line parsing library.
/// </summary>
public static class DemoDataCommandLine
{
    /// <summary>The switch that names the mandatory output directory.</summary>
    public const string OutSwitch = "--out";

    /// <summary>The switch that names the last day the export covers.</summary>
    public const string UntilSwitch = "--until";

    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Parses <paramref name="args"/> into <see cref="DemoDataOptions"/>, or
    /// reports the one reason it could not.
    /// </summary>
    /// <param name="args">The arguments the process was started with.</param>
    /// <param name="today">What <see cref="UntilSwitch"/> defaults to when it is not given.</param>
    public static DemoDataCommandLineResult Parse(IReadOnlyList<string> args, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(args);

        string? outputDirectory = null;
        var until = today;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];

            switch (argument)
            {
                case OutSwitch:
                    if (index + 1 >= args.Count)
                    {
                        return DemoDataCommandLineResult.Failure($"{OutSwitch} requires a directory.");
                    }

                    outputDirectory = args[++index];
                    break;

                case UntilSwitch:
                    if (index + 1 >= args.Count)
                    {
                        return DemoDataCommandLineResult.Failure($"{UntilSwitch} requires a date.");
                    }

                    var untilText = args[++index];

                    if (!DateOnly.TryParseExact(
                            untilText,
                            DateFormat,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out until))
                    {
                        return DemoDataCommandLineResult.Failure(
                            $"{UntilSwitch} requires a date shaped {DateFormat}, got '{untilText}'.");
                    }

                    break;

                default:
                    return DemoDataCommandLineResult.Failure($"Unknown argument: {argument}");
            }
        }

        if (outputDirectory is null)
        {
            return DemoDataCommandLineResult.Failure($"{OutSwitch} <directory> is required.");
        }

        return DemoDataCommandLineResult.Success(new DemoDataOptions(outputDirectory, until));
    }
}
