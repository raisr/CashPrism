using System.Globalization;
using System.Text.RegularExpressions;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Builds a shape-preserving replacement for one value in the account/IBAN
/// dictionary. <c>IBAN Beguenstigter/Auftraggeber</c> carries four shapes in a
/// real export — an IBAN, a UUID, an email address, and a short opaque code
/// (see <c>docs/finanzguru-export.md</c>) — but the ticket only asks for two of
/// them to keep their recognisable shape: an IBAN stays IBAN-shaped, an email
/// address stays email-shaped. Anything else falls back to a length-preserving
/// numeric placeholder, which is honest about not knowing the shape rather than
/// guessing one.
/// </summary>
/// <remarks>
/// Generated IBANs and the numeric fallback carry no valid check digit — see
/// <c>docs/anonymiser.md</c>. That is deliberate, not a shortcut: a real check
/// digit would tempt a future reader into trusting the placeholder as bookable
/// data.
/// </remarks>
internal static class ValueShape
{
    private static readonly Regex IbanPattern = new(
        "^[A-Z]{2}[0-9]{2}[A-Za-z0-9]{4,30}$", RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(
        """^[^@\s]+@[^@\s]+\.[^@\s]+$""", RegexOptions.Compiled);

    /// <summary>
    /// Builds the replacement for <paramref name="original"/>. <paramref name="index"/>
    /// is this value's 1-based position in its dictionary — it is what makes the
    /// same value always yield the same replacement, and different values yield
    /// different ones. <paramref name="emailWidth"/> is the digit width used only
    /// for the email shape, computed once from the dictionary's total size so
    /// every generated address is padded alike regardless of assignment order.
    /// </summary>
    public static string BuildReplacement(string original, int index, int emailWidth)
    {
        if (IbanPattern.IsMatch(original))
        {
            return BuildFakeIban(original, index);
        }

        if (EmailPattern.IsMatch(original))
        {
            return BuildFakeEmail(index, emailWidth);
        }

        return BuildFallback(original, index);
    }

    private static string BuildFakeIban(string original, int index)
    {
        var countryCode = original[..2];
        var remainingLength = original.Length - 2;
        var digits = index.ToString(CultureInfo.InvariantCulture);

        return countryCode + PadOrTruncateToLength(digits, remainingLength);
    }

    private static string BuildFakeEmail(int index, int emailWidth)
    {
        var digits = index.ToString(CultureInfo.InvariantCulture).PadLeft(emailWidth, '0');

        return $"account-{digits}@example.invalid";
    }

    private static string BuildFallback(string original, int index)
    {
        var digits = index.ToString(CultureInfo.InvariantCulture);

        return PadOrTruncateToLength(digits, original.Length);
    }

    /// <summary>
    /// Zero-pads <paramref name="digits"/> to exactly <paramref name="length"/>
    /// characters, or keeps only its low-order digits when it is already longer
    /// — a dictionary large enough to overflow the available digits is not
    /// expected in practice, and truncating from the left keeps the mapping at
    /// least injective within one run's realistic size.
    /// </summary>
    private static string PadOrTruncateToLength(string digits, int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        return digits.Length >= length ? digits[^length..] : digits.PadLeft(length, '0');
    }
}
