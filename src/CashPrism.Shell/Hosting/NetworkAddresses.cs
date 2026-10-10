using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CashPrism.Shell.Hosting;

/// <summary>
/// The addresses of this machine, reduced to the ones another device in the same
/// home network can actually reach.
/// </summary>
public static class NetworkAddresses
{
    /// <summary>
    /// The unicast addresses of every operational, non-loopback interface,
    /// unfiltered. Which of them is "the right one" is not decided here: VPN,
    /// Hyper-V and Docker adapters make that guess wrong often enough that the
    /// banner reports all of them.
    /// </summary>
    public static IReadOnlyList<IPAddress> Local()
    {
        return
        [
            .. NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Select(a => a.Address),
        ];
    }

    /// <summary>
    /// Keeps the IPv4 addresses in the private ranges of RFC 1918 — 10/8,
    /// 172.16/12 and 192.168/16 — in the order they came in. Everything else is
    /// dropped: loopback, link-local, public and IPv6 addresses.
    /// </summary>
    public static IReadOnlyList<IPAddress> FilterPrivateIPv4(IEnumerable<IPAddress> addresses)
    {
        ArgumentNullException.ThrowIfNull(addresses);

        return [.. addresses.Where(IsPrivateIPv4)];
    }

    /// <summary>
    /// One <c>http://address:port</c> for each address of
    /// <paramref name="addresses"/> another device in the home network can
    /// reach, as <see cref="FilterPrivateIPv4"/> decides. The start banner and
    /// the settings page both list these, so they cannot drift apart.
    /// </summary>
    public static IReadOnlyList<string> Urls(int port, IEnumerable<IPAddress> addresses)
        => [.. FilterPrivateIPv4(addresses).Select(address => $"http://{address}:{port}")];

    private static bool IsPrivateIPv4(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }

        var octets = address.GetAddressBytes();

        return octets[0] switch
        {
            10 => true,
            172 => octets[1] is >= 16 and <= 31,
            192 => octets[1] == 168,
            _ => false,
        };
    }
}
