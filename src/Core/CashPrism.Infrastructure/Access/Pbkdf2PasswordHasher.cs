using System.Globalization;
using System.Security.Cryptography;
using CashPrism.Application.Access;

namespace CashPrism.Infrastructure.Access;

/// <summary>
/// Hashes passwords with PBKDF2 from the BCL. The stored value names the
/// algorithm and the iteration count beside the salt and the hash, so a later
/// build can raise the count and still check what an earlier one stored.
/// </summary>
/// <remarks>
/// The format is <c>pbkdf2-sha512$&lt;iterations&gt;$&lt;salt&gt;$&lt;hash&gt;</c>,
/// salt and hash in Base64. The iteration count follows the OWASP
/// recommendation for PBKDF2-HMAC-SHA512.
/// </remarks>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "pbkdf2-sha512";
    private const int Iterations = 210_000;
    private const int SaltSizeInBytes = 16;
    private const int HashSizeInBytes = 32;
    private const char Separator = '$';

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);
        var hash = Derive(password, salt, Iterations, HashSizeInBytes);

        return string.Join(
            Separator,
            Algorithm,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(hash);

        var parts = hash.Split(Separator);

        if (parts.Length != 4
            || parts[0] != Algorithm
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Derive(password, salt, iterations, expected.Length);

        // In constant time, so how long a wrong password takes says nothing
        // about how much of it was right.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int length)
        => Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, length);
}
