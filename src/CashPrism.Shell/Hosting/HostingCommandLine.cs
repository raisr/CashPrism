namespace CashPrism.Shell.Hosting;

/// <summary>
/// The command-line surface of the executable: <c>--port &lt;number&gt;</c> and
/// <c>--no-browser</c>, both feeding the <c>Hosting</c> configuration section.
/// </summary>
public static class HostingCommandLine
{
    /// <summary>The valueless flag that suppresses the browser launch.</summary>
    public const string NoBrowserSwitch = "--no-browser";

    /// <summary>
    /// Maps the short switches onto configuration keys. Only switches that carry
    /// a value belong here — see <see cref="Expand"/>.
    /// </summary>
    public static IDictionary<string, string> CreateSwitchMappings()
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["--port"] = $"{HostingOptions.SectionName}:{nameof(HostingOptions.Port)}",
        };
    }

    /// <summary>
    /// Rewrites <see cref="NoBrowserSwitch"/> into an explicit key/value
    /// argument. The command-line configuration provider has no concept of a
    /// valueless flag and fails silently rather than loudly: a trailing
    /// <c>--no-browser</c> is dropped, and otherwise the next argument is
    /// consumed as its value, so <c>--no-browser --port 5099</c> would lose the
    /// port.
    /// </summary>
    public static string[] Expand(IEnumerable<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var noBrowser =
            $"--{HostingOptions.SectionName}:{nameof(HostingOptions.LaunchBrowser)}=false";

        return [.. args.Select(a => a == NoBrowserSwitch ? noBrowser : a)];
    }
}
