using System.Net;
using CashPrism.Application.Access;
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
    /// <paramref name="addresses"/> that is reachable from the home network, and
    /// the setup code while no password is set.
    /// </summary>
    /// <param name="port">The port the server listens on.</param>
    /// <param name="addresses">The addresses of the machine.</param>
    /// <param name="setupCode">The code that sets the first password, or <c>null</c> once one is set.</param>
    public static IReadOnlyList<string> Compose(int port, IEnumerable<IPAddress> addresses, SetupCode? setupCode = null)
    {
        ArgumentNullException.ThrowIfNull(addresses);

        List<string> lines =
        [
            $"CashPrism {AppVersion.Current}",
            string.Empty,
            "On this machine:",
            $"  http://localhost:{port}",
        ];

        var reachable = NetworkAddresses.Urls(port, addresses);

        if (reachable.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("From another device in the same network:");
            lines.AddRange(reachable.Select(url => $"  {url}"));
        }

        AddSetupCode(lines, setupCode);

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
    /// <param name="port">The port the server listens on inside the container.</param>
    /// <param name="setupCode">The code that sets the first password, or <c>null</c> once one is set.</param>
    public static IReadOnlyList<string> ComposeForContainer(int port, SetupCode? setupCode = null)
    {
        List<string> lines =
        [
            $"CashPrism {AppVersion.Current}",
            string.Empty,
            $"Listening on port {port} inside the container.",
            "Open the host port it is published on, for example",
            $"  http://localhost:{port} when started with -p {port}:{port}",
        ];

        AddSetupCode(lines, setupCode);

        lines.Add(string.Empty);
        lines.Add("Press Ctrl+C to stop.");

        return lines;
    }

    /// <summary>
    /// The code that sets the first password, while there is none. In a
    /// container this lands in its log, and that is deliberate: whoever reads
    /// the log has the machine — see the deviation from <c>core.security</c>
    /// in <c>AGENTS.md</c>.
    /// </summary>
    private static void AddSetupCode(List<string> lines, SetupCode? setupCode)
    {
        if (setupCode is null)
        {
            return;
        }

        lines.Add(string.Empty);
        lines.Add("No password is set yet. Open CashPrism and enter this setup code:");
        lines.Add($"  {setupCode.Display}");
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
