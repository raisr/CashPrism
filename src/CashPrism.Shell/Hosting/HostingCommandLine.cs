namespace CashPrism.Shell.Hosting;

/// <summary>
/// The command-line surface of the executable: <c>--port &lt;number&gt;</c>,
/// <c>--no-browser</c> and <c>--reset-password</c>, all feeding the
/// <c>Hosting</c> configuration section.
/// </summary>
public static class HostingCommandLine
{
    /// <summary>The valueless flag that suppresses the browser launch.</summary>
    public const string NoBrowserSwitch = "--no-browser";

    /// <summary>The valueless flag that drops the password before serving.</summary>
    public const string ResetPasswordSwitch = "--reset-password";

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
    /// Rewrites the valueless flags into explicit key/value arguments. The
    /// command-line configuration provider has no concept of a valueless flag
    /// and fails silently rather than loudly: a trailing <c>--no-browser</c> is
    /// dropped, and otherwise the next argument is consumed as its value, so
    /// <c>--no-browser --port 5099</c> would lose the port.
    /// </summary>
    public static string[] Expand(IEnumerable<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var flags = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [NoBrowserSwitch] = $"--{HostingOptions.SectionName}:{nameof(HostingOptions.LaunchBrowser)}=false",
            [ResetPasswordSwitch] = $"--{HostingOptions.SectionName}:{nameof(HostingOptions.ResetPassword)}=true",
        };

        return [.. args.Select(a => flags.GetValueOrDefault(a, a))];
    }
}
