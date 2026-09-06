namespace CashPrism.Shell.Hosting;

/// <summary>
/// The hosting settings a person can change without recompiling: the port the
/// server listens on, where the data directory lives and whether the local
/// browser opens on start. Bound from the <c>Hosting</c> configuration section.
/// </summary>
public sealed record HostingOptions
{
    /// <summary>The configuration section these options are bound from.</summary>
    public const string SectionName = "Hosting";

    /// <summary>
    /// The TCP port Kestrel listens on, on every network interface. Overridable
    /// with <c>--port</c>.
    /// </summary>
    public int Port { get; init; } = 5080;

    /// <summary>
    /// The directory holding the database and the stored import files. A relative
    /// path is resolved against the directory of the executable rather than the
    /// working directory: a single-file binary is typically started from
    /// somewhere else, and the README promises the database sits next to the
    /// application.
    /// </summary>
    public string DataDirectory { get; init; } = "data";

    /// <summary>
    /// Whether to open the local browser once the server listens. Switched off
    /// with <c>--no-browser</c>.
    /// </summary>
    public bool LaunchBrowser { get; init; } = true;

    /// <summary>
    /// <see cref="DataDirectory"/> as an absolute path. An absolute setting is
    /// taken as it is.
    /// </summary>
    public string ResolveDataDirectory()
    {
        return Path.GetFullPath(DataDirectory, AppContext.BaseDirectory);
    }
}
