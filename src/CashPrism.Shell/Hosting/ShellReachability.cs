using System.Net;
using CashPrism.Web.Network;

namespace CashPrism.Shell.Hosting;

/// <summary>
/// Tells the settings page what the start banner prints: the same addresses,
/// composed the same way, and the same decision about a container.
/// </summary>
/// <param name="port">The port the server listens on.</param>
/// <param name="inContainer">Whether the process runs in a container.</param>
/// <param name="addresses">
/// Reads the machine's addresses. Asked on every read, so a laptop that moved
/// to another network shows where it is now.
/// </param>
public sealed class ShellReachability(int port, bool inContainer, Func<IEnumerable<IPAddress>> addresses) : IReachability
{
    /// <inheritdoc />
    public Reachability Read()
        => inContainer
            ? new Reachability(port, InContainer: true, [])
            : new Reachability(port, InContainer: false, NetworkAddresses.Urls(port, addresses()));
}
