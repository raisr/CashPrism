using CashPrism.Application.Time;

namespace CashPrism.Application.Tests.Unit.Imports;

/// <summary>A clock that stands still where a test put it.</summary>
/// <param name="utcNow">The instant it reports.</param>
internal sealed class FixedClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}
