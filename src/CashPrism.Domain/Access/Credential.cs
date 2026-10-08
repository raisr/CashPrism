namespace CashPrism.Domain.Access;

/// <summary>
/// The one password the household shares, as a hash. CashPrism has no user
/// accounts, so there is at most one of these.
/// </summary>
/// <remarks>
/// A reset keeps the credential and drops only the hash, because the
/// <see cref="Generation"/> has to outlive it: a login carries the generation it
/// was issued under, and a counter that started again at one after a reset would
/// let a login from before it count again.
/// </remarks>
public sealed class Credential
{
    /// <summary>The generation of a credential whose password was set once and never changed.</summary>
    public const int FirstGeneration = 1;

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
        Generation = FirstGeneration;
    }

    // For reading a stored credential back, which may be one without a hash:
    // given the choice, the database layer binds the constructor with the fewest
    // parameters, and the public one would refuse a reset credential.
    private Credential()
    {
    }

    /// <summary>The password, hashed, or <c>null</c> after a <see cref="Reset"/>.</summary>
    public string? PasswordHash { get; private set; }

    /// <summary>When the password was last set.</summary>
    public DateTimeOffset SetAt { get; private set; }

    /// <summary>
    /// Counts the changes to the password. Every change and every reset raises
    /// it, and a login issued under a lower one is no longer accepted.
    /// </summary>
    public int Generation { get; private set; }

    /// <summary>Whether a password is set, so a login is possible.</summary>
    public bool IsPasswordSet => PasswordHash is not null;

    /// <summary>
    /// Replaces the password — a change, or the first password after a
    /// <see cref="Reset"/>.
    /// </summary>
    /// <param name="passwordHash">The new password, hashed.</param>
    /// <param name="setAt">When it was set.</param>
    /// <exception cref="ArgumentException"><paramref name="passwordHash"/> is missing.</exception>
    public void SetPassword(string passwordHash, DateTimeOffset setAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        SetAt = setAt;
        Generation++;
    }

    /// <summary>
    /// Drops the password, so CashPrism waits for a new one as on its first start.
    /// </summary>
    /// <exception cref="InvalidOperationException">No password is set.</exception>
    public void Reset()
    {
        if (!IsPasswordSet)
        {
            throw new InvalidOperationException("No password is set, so there is none to reset.");
        }

        PasswordHash = null;
        Generation++;
    }
}
