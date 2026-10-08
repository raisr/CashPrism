namespace CashPrism.Application.Access;

/// <summary>
/// Drops a forgotten password, so CashPrism waits for a new one as on its first
/// start. Every login issued before it stops counting; nothing else that is
/// stored is touched.
/// </summary>
/// <remarks>
/// There is no account and no email to prove who asks, so only whoever starts
/// the application can: the host offers this as a start switch, never in the
/// browser.
/// </remarks>
public sealed class PasswordReset
{
    private readonly ICredentialStore store;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="store">Where the credential is kept.</param>
    public PasswordReset(ICredentialStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        this.store = store;
    }

    /// <summary>Drops the password, if one is set.</summary>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>Whether there was a password to drop.</returns>
    public async Task<bool> ResetAsync(CancellationToken cancellationToken = default)
    {
        if (await store.GetAsync(cancellationToken) is not { IsPasswordSet: true } credential)
        {
            return false;
        }

        credential.Reset();
        await store.UpdateAsync(credential, cancellationToken);

        return true;
    }
}
