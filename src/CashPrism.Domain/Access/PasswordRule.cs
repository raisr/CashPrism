namespace CashPrism.Domain.Access;

/// <summary>
/// What a password has to be to protect CashPrism: long enough, and nothing
/// else. Composition rules make people write passwords down rather than make
/// them stronger; length is what makes guessing expensive.
/// </summary>
public static class PasswordRule
{
    /// <summary>The fewest characters a password may have.</summary>
    public const int MinimumLength = 10;

    /// <summary>Whether <paramref name="password"/> is long enough to be accepted.</summary>
    /// <param name="password">The password as typed.</param>
    public static bool IsAcceptable(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        return password.Length >= MinimumLength;
    }
}
