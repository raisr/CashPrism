using CashPrism.Application.Time;

namespace CashPrism.Application.Access;

/// <summary>
/// Slows down guessing the password. After <see cref="FailuresPerLockout"/>
/// wrong passwords in a row, logins are refused for <see cref="FirstLockout"/>;
/// every further lockout lasts twice as long as the one before, up to
/// <see cref="LongestLockout"/>. A correct password forgets all of it.
/// </summary>
/// <remarks>
/// <para>
/// Counted for the whole application, not per address: behind a container's
/// published port every device arrives from the same gateway address, so
/// counting per address would be global anyway, only less honest about it. The
/// flip side is that someone guessing locks the household out as well — for at
/// most <see cref="LongestLockout"/>.
/// </para>
/// <para>
/// In memory only, so a restart forgets it. Whoever can restart CashPrism has
/// the machine already.
/// </para>
/// </remarks>
public sealed class LoginThrottle
{
    /// <summary>How many wrong passwords in a row lead to a lockout.</summary>
    public const int FailuresPerLockout = 5;

    /// <summary>How long the first lockout lasts.</summary>
    public static readonly TimeSpan FirstLockout = TimeSpan.FromMinutes(1);

    /// <summary>How long a lockout lasts at most.</summary>
    public static readonly TimeSpan LongestLockout = TimeSpan.FromMinutes(15);

    private readonly IClock clock;
    private readonly Lock gate = new();

    private int failures;
    private int lockouts;
    private DateTimeOffset? lockedUntil;

    /// <summary>
    /// Creates the throttle.
    /// </summary>
    /// <param name="clock">Says when a lockout ends.</param>
    public LoginThrottle(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        this.clock = clock;
    }

    /// <summary>When the current lockout ends, or <c>null</c> when logins are accepted.</summary>
    public DateTimeOffset? LockedUntil()
    {
        lock (gate)
        {
            return IsLocked() ? lockedUntil : null;
        }
    }

    /// <summary>
    /// Counts a wrong password. One attempted during a lockout is not counted:
    /// it was refused without being checked.
    /// </summary>
    public void RecordFailure()
    {
        lock (gate)
        {
            if (IsLocked())
            {
                return;
            }

            failures++;

            if (failures < FailuresPerLockout)
            {
                return;
            }

            failures = 0;
            lockouts++;
            lockedUntil = clock.UtcNow + LockoutDuration(lockouts);
        }
    }

    /// <summary>Forgets every failure and lockout so far.</summary>
    public void RecordSuccess()
    {
        lock (gate)
        {
            failures = 0;
            lockouts = 0;
            lockedUntil = null;
        }
    }

    private static TimeSpan LockoutDuration(int lockout)
    {
        // Doubling from one minute passes the ceiling at the fifth lockout; the
        // cap on the exponent keeps the arithmetic finite far beyond it.
        var doubled = FirstLockout * Math.Pow(2, Math.Min(lockout - 1, 16));

        return doubled < LongestLockout ? doubled : LongestLockout;
    }

    private bool IsLocked() => lockedUntil is { } until && clock.UtcNow < until;
}
