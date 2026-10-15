namespace CashPrism.Application.Access;

/// <summary>
/// Decides whether a login handed out earlier still counts: it does as long as
/// the password it was issued under has not been changed or reset since.
/// </summary>
public sealed class LoginValidation
{
    private readonly ICredentialStore store;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="store">Where the credential is kept.</param>
    public LoginValidation(ICredentialStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        this.store = store;
    }

    /// <summary>Whether a login issued under <paramref name="generation"/> still counts.</summary>
    /// <param name="generation">The generation the login carries.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<bool> IsCurrentAsync(int generation, CancellationToken cancellationToken = default)
    {
        return await store.GetAsync(cancellationToken) is { IsPasswordSet: true } credential
            && credential.Generation == generation;
    }
}
