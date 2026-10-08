namespace CashPrism.Application.Access;

/// <summary>
/// Turns a password into something that can be stored and checked, but not
/// read back. Behind an interface because a hash is salted, and the salt is
/// randomness the application does not produce itself.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes <paramref name="password"/> with a fresh salt.</summary>
    /// <param name="password">The password as typed.</param>
    string Hash(string password);

    /// <summary>Whether <paramref name="password"/> is the one <paramref name="hash"/> was made from.</summary>
    /// <param name="password">The password as typed.</param>
    /// <param name="hash">A value <see cref="Hash"/> returned.</param>
    bool Verify(string password, string hash);
}
