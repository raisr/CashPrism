using System.Net;
using CashPrism.Web.Configuration;

namespace CashPrism.Shell.Hosting;

/// <summary>
/// The lines printed once the server listens. This is user-facing console
/// output, not logging — the documented exception to the <c>ILogger&lt;T&gt;</c>
/// rule in <c>AGENTS.md</c>: someone reads an address off this screen to type it
/// into a phone.
/// </summary>
public static class StartBanner
{
    /// <summary>
    /// Builds the banner: the loopback URL, plus one URL per address of
    /// <paramref name="addresses"/> that is reachable from the home network.
    /// </summary>
    public static IReadOnlyList<string> Compose(int port, IEnumerable<IPAddress> addresses)
    {
        ArgumentNullException.ThrowIfNull(addresses);

        List<string> lines =
        [
            $"CashPrism {AppVersion.Current}",
            string.Empty,
            "On this machine:",
            $"  http://localhost:{port}",
        ];

        var reachable = NetworkAddresses.FilterPrivateIPv4(addresses);

        if (reachable.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("From another device in the same network:");
            lines.AddRange(reachable.Select(a => $"  http://{a}:{port}"));
        }

        lines.Add(string.Empty);
        lines.Add("Press Ctrl+C to stop.");

        return lines;
    }

    /// <summary>Writes <see cref="Compose"/> to the console.</summary>
    public static void Print(int port, IEnumerable<IPAddress> addresses)
    {
        foreach (var line in Compose(port, addresses))
        {
            Console.WriteLine(line);
        }
    }
}
