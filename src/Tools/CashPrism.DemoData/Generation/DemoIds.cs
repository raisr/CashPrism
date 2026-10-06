using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// Identifiers and digits derived from a key by hashing it, so the same key
/// always gives the same value without a random number generator in between.
/// </summary>
public static class DemoIds
{
    /// <summary>
    /// The <c>Buchungs-ID</c> of the <paramref name="sequence"/>-th generated
    /// booking: 40 lower-case hexadecimal characters, as the export carries it.
    /// </summary>
    public static string BookingId(int sequence)
        => Hex("booking|" + sequence.ToString(CultureInfo.InvariantCulture), 40);

    /// <summary>
    /// The <c>Analyse-Vertrags-ID</c> of the contract named <paramref name="contract"/>:
    /// 32 lower-case hexadecimal characters, a UUID without its dashes.
    /// </summary>
    public static string ContractId(string contract) => Hex("contract|" + contract, 32);

    /// <summary>A lower-case UUID with dashes, derived from <paramref name="key"/>.</summary>
    public static string Uuid(string key) => new Guid(Hex("uuid|" + key, 32)).ToString("D");

    /// <summary>A string of <paramref name="count"/> decimal digits derived from <paramref name="key"/>.</summary>
    public static string Digits(string key, int count)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var digits = new StringBuilder(count);

        for (var index = 0; digits.Length < count; index++)
        {
            digits.Append((char)('0' + ((hash[index % hash.Length] + index) % 10)));
        }

        return digits.ToString();
    }

    private static string Hex(string key, int length)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..length];
}
