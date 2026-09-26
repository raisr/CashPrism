namespace CashPrism.Application.Time;

/// <summary>
/// The current time, as the application reads it. Injected rather than read from
/// <see cref="DateTimeOffset.UtcNow"/>, so that anything which records a time can
/// be tested without waiting for one.
/// </summary>
public interface IClock
{
    /// <summary>The current instant, in UTC.</summary>
    DateTimeOffset UtcNow { get; }
}
