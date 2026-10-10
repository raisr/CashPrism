using CashPrism.Web.Network;

namespace CashPrism.Web.Tests.Unit.Network;

/// <summary>Tells a page whatever the test sets.</summary>
public sealed class FakeReachability : IReachability
{
    /// <summary>What <see cref="Read"/> returns.</summary>
    public Reachability Current { get; set; } = new(5080, InContainer: false, ["http://192.168.1.5:5080"]);

    /// <inheritdoc />
    public Reachability Read() => Current;
}
