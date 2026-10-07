namespace CashPrism.Domain.Access;

/// <summary>
/// The one password the household shares, as a hash. CashPrism has no user
/// accounts, so there is at most one of these.
/// </summary>
public sealed class Credential
{
    /// <summary>
    /// Creates a credential.
    /// </summary>
    /// <param name="passwordHash">The password, hashed. Never the password itself.</param>
    /// <param name="setAt">When the password was set.</param>
    /// <exception cref="ArgumentException"><paramref name="passwordHash"/> is missing.</exception>
    public Credential(string passwordHash, DateTimeOffset setAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        SetAt = setAt;
    }

    /// <summary>The password, hashed.</summary>
    public string PasswordHash { get; }

    /// <summary>When the password was set.</summary>
    public DateTimeOffset SetAt { get; }
}
