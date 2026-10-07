using System.Net;
using CashPrism.Web.Configuration;

namespace CashPrism.Shell.Hosting;

/// <summary>
/// The lines printed once the server listens. This is user-facing console
/// output, not logging — the documented exception to the <c>ILogger&lt;T&gt;</c>
/// rule in <c>Agents.md</c>: someone reads an address off this screen to type it
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

    /// <summary>
    /// Builds the banner for a process running in a container. The container's
    /// own addresses are reachable from nowhere but the container network, and
    /// which host port maps onto <paramref name="port"/> is known only to whoever
    /// started it, so the banner names the port and leaves the address to them.
    /// </summary>
    public static IReadOnlyList<string> ComposeForContainer(int port)
    {
        return
        [
            $"CashPrism {AppVersion.Current}",
            string.Empty,
            $"Listening on port {port} inside the container.",
            "Open the host port it is published on, for example",
            $"  http://localhost:{port} when started with -p {port}:{port}",
            string.Empty,
            "Press Ctrl+C to stop.",
        ];
    }

    /// <summary>Writes a banner from <see cref="Compose"/> or <see cref="ComposeForContainer"/> to the console.</summary>
    public static void Print(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
    }
}
