using CashPrism.Application.Time;

namespace CashPrism.Infrastructure.Time;

/// <summary>
/// The clock of the machine CashPrism runs on. The only implementation that is
/// not a test double, and the only place in the application that reads the time.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
