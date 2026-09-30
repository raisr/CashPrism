using CashPrism.Application.Time;

namespace CashPrism.TestSupport.Imports;

/// <summary>A clock that stands still where a test put it.</summary>
/// <param name="utcNow">The instant it reports.</param>
public sealed class FixedClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}
