using CashPrism.Application.Time;

namespace CashPrism.Application.Tests.Unit.Access;

/// <summary>A clock a test moves forward by hand, for anything that waits.</summary>
internal sealed class SteppingClock : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = new(2026, 10, 7, 20, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan by) => UtcNow += by;
}
