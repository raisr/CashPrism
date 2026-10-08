namespace CashPrism.Application.Access;

/// <summary>
/// Checks a password typed at the login against the stored one, through the
/// <see cref="LoginThrottle"/>. Signing in on success is the host's business.
/// </summary>
public sealed class PasswordCheck
{
    private readonly ICredentialStore store;
    private readonly IPasswordHasher hasher;
    private readonly LoginThrottle throttle;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="store">Where the credential is kept.</param>
    /// <param name="hasher">Checks the password against the stored hash.</param>
    /// <param name="throttle">Counts wrong passwords.</param>
    public PasswordCheck(ICredentialStore store, IPasswordHasher hasher, LoginThrottle throttle)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(hasher);
        ArgumentNullException.ThrowIfNull(throttle);

        this.store = store;
        this.hasher = hasher;
        this.throttle = throttle;
    }

    /// <summary>Checks <paramref name="password"/>.</summary>
    /// <param name="password">The password as typed.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<LoginResult> CheckAsync(string password, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (throttle.LockedUntil() is { } lockedUntil)
        {
            return new LoginResult(LoginOutcome.LockedOut, lockedUntil);
        }

        // No password set means none can be right. Not counted: there is
        // nothing to guess yet, and the setup page is where this ends.
        if (await store.GetAsync(cancellationToken) is not { PasswordHash: { } passwordHash })
        {
            return new LoginResult(LoginOutcome.Rejected, LockedUntil: null);
        }

        if (hasher.Verify(password, passwordHash))
        {
            throttle.RecordSuccess();

            return new LoginResult(LoginOutcome.Accepted, LockedUntil: null);
        }

        throttle.RecordFailure();

        return new LoginResult(LoginOutcome.Rejected, throttle.LockedUntil());
    }
}
