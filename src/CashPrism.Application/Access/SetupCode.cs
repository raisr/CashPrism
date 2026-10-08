using System.Security.Cryptography;
using System.Text;

namespace CashPrism.Application.Access;

/// <summary>
/// The code that allows setting the first password. It is printed where only
/// whoever runs CashPrism sees it — the console, or the log of a container — so
/// knowing it shows that the person setting the password has the machine, not
/// just the Wi-Fi.
/// </summary>
/// <remarks>
/// A new one is made on every start, by the host: it is random, and randomness
/// is not something the application produces itself. It lives in memory only.
/// </remarks>
public sealed class SetupCode
{
    /// <summary>The characters a code is made of: no 0/O or 1/I/L, so it can be read off a screen.</summary>
    public const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    /// <summary>How many characters a code has.</summary>
    public const int Length = 12;

    private const int GroupLength = 4;

    private readonly string value;

    /// <summary>
    /// Creates the code.
    /// </summary>
    /// <param name="value"><see cref="Length"/> characters out of <see cref="Alphabet"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a code of that shape.</exception>
    public SetupCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length != Length || !value.All(Alphabet.Contains))
        {
            throw new ArgumentException(
                $"A setup code is {Length} characters out of {Alphabet}.", nameof(value));
        }

        this.value = value;
    }

    /// <summary>The code as it is printed: in groups of four, so it can be typed off a screen.</summary>
    public string Display => string.Join('-', value.Chunk(GroupLength).Select(group => new string(group)));

    /// <summary>
    /// Whether <paramref name="candidate"/> is this code. Case, spaces and dashes
    /// do not matter: the code is read off one screen and typed into another.
    /// </summary>
    /// <param name="candidate">What was typed, or <c>null</c> when nothing was.</param>
    public bool Matches(string? candidate)
    {
        if (candidate is null)
        {
            return false;
        }

        var typed = new string([.. candidate.Where(c => c is not ('-' or ' ')).Select(char.ToUpperInvariant)]);

        // In constant time, so how long a wrong guess takes says nothing about
        // how much of it was right.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(typed),
            Encoding.UTF8.GetBytes(value));
    }

    /// <inheritdoc />
    public override string ToString() => Display;
}
