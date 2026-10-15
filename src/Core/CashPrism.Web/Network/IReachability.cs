namespace CashPrism.Web.Network;

/// <summary>
/// Where another device in the household reaches CashPrism. Only the host
/// knows the port it bound and the machine it runs on, so it implements this;
/// the settings page shows what it is told.
/// </summary>
public interface IReachability
{
    /// <summary>The addresses as they stand now, not as they stood at start.</summary>
    Reachability Read();
}
