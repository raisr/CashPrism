using CashPrism.Application.Time;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Access;

/// <summary>
/// Replaces the password, for whoever knows the current one. Every login issued
/// before the change stops counting; signing this device out as well is the
/// host's business.
/// </summary>
public sealed class PasswordChange
{
    private readonly PasswordCheck check;
    private readonly ICredentialStore store;
    private readonly IPasswordHasher hasher;
    private readonly IClock clock;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="check">
    /// Checks the current password, through the same throttle as the login: a
    /// wrong one here is a guess like any other.
    /// </param>
    /// <param name="store">Where the credential is kept.</param>
    /// <param name="hasher">Hashes the new password before it is kept.</param>
    /// <param name="clock">Says when the password was set.</param>
    public PasswordChange(PasswordCheck check, ICredentialStore store, IPasswordHasher hasher, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(check);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(hasher);
        ArgumentNullException.ThrowIfNull(clock);

        this.check = check;
        this.store = store;
        this.hasher = hasher;
        this.clock = clock;
    }

    /// <summary>
    /// Sets <paramref name="newPassword"/> as the password, if
    /// <paramref name="currentPassword"/> is the one set now.
    /// </summary>
    /// <param name="currentPassword">The current password as typed.</param>
    /// <param name="newPassword">The new password as typed.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    public async Task<PasswordChangeResult> ChangeAsync(
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentPassword);
        ArgumentNullException.ThrowIfNull(newPassword);

        var login = await check.CheckAsync(currentPassword, cancellationToken);

        switch (login.Outcome)
        {
            case LoginOutcome.LockedOut:
                return new PasswordChangeResult(PasswordChangeOutcome.LockedOut, login.LockedUntil);
            case LoginOutcome.Rejected:
                return new PasswordChangeResult(PasswordChangeOutcome.Rejected, login.LockedUntil);
        }

        if (!PasswordRule.IsAcceptable(newPassword))
        {
            return new PasswordChangeResult(PasswordChangeOutcome.TooShort, LockedUntil: null);
        }

        // The check just found it, so it is there; read again to write it back.
        var credential = await store.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("The credential disappeared between check and change.");

        credential.SetPassword(hasher.Hash(newPassword), clock.UtcNow);
        await store.UpdateAsync(credential, cancellationToken);

        return new PasswordChangeResult(PasswordChangeOutcome.Done, LockedUntil: null);
    }
}
