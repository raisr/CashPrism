using System.ComponentModel;
using System.Diagnostics;

namespace CashPrism.Shell.Hosting;

/// <summary>
/// Opens the default browser on the machine hosting the application. Best effort
/// throughout: the application is reachable either way, so a headless machine or
/// a missing <c>xdg-open</c> must never take the process down.
/// </summary>
public static class BrowserLauncher
{
    /// <summary>
    /// Opens <paramref name="url"/> in the default browser, logging a warning
    /// instead of throwing when the platform refuses.
    /// </summary>
    public static void Open(string url, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            using var process = Process.Start(StartInfoFor(url));
        }
        catch (Exception exception) when (
            exception is Win32Exception
                or InvalidOperationException
                or PlatformNotSupportedException)
        {
            logger.LogWarning(exception, "Could not open a browser for {Url}", url);
        }
    }

    private static ProcessStartInfo StartInfoFor(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            // Only the shell knows which program is registered for http.
            return new ProcessStartInfo(url) { UseShellExecute = true };
        }

        return OperatingSystem.IsMacOS()
            ? new ProcessStartInfo("open", url)
            : new ProcessStartInfo("xdg-open", url);
    }
}
