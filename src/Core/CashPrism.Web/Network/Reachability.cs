namespace CashPrism.Web.Network;

/// <summary>Where CashPrism can be reached from another device.</summary>
/// <param name="Port">The port the server listens on.</param>
/// <param name="InContainer">
/// Whether the process runs in a container. Its own addresses then belong to
/// the container network, so <paramref name="Urls"/> is empty and the address
/// is the host's.
/// </param>
/// <param name="Urls">One <c>http://address:port</c> per address of the home network.</param>
public sealed record Reachability(int Port, bool InContainer, IReadOnlyList<string> Urls);
