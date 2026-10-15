using System.Globalization;
using System.Text;

namespace CashPrism.Application.Bookings;

/// <summary>
/// Brings a text into the one form the booking search compares in, so what a
/// person types and what the export carries meet however either is spelled.
/// </summary>
/// <remarks>
/// <para>
/// Three steps, in this order. Upper case, the same in every culture. Then the
/// German spelling without umlauts: banks pass a counterparty and a payment
/// reference through the SEPA character set, which has none, so the export
/// carries "MOEBEL" where a person types "möbel" — both have to end up the
/// same. Then every other accent goes, so "cafe" finds "Café".
/// </para>
/// <para>
/// The German rule comes first and wins: a Swedish "ö" a person types as "o"
/// is not found. It is decided again once a source in another language
/// arrives. Letters Unicode does not take apart, such as "ø", stay as they
/// are.
/// </para>
/// </remarks>
public static class SearchText
{
    /// <summary>The form <paramref name="text"/> is compared in.</summary>
    /// <param name="text">What a person typed, or a stored field.</param>
    public static string Fold(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var upper = text.Normalize(NormalizationForm.FormC).ToUpperInvariant()
            .Replace("Ä", "AE", StringComparison.Ordinal)
            .Replace("Ö", "OE", StringComparison.Ordinal)
            .Replace("Ü", "UE", StringComparison.Ordinal)
            .Replace("ß", "SS", StringComparison.Ordinal)
            .Replace("ẞ", "SS", StringComparison.Ordinal);

        var folded = new StringBuilder(upper.Length);

        foreach (var character in upper.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                folded.Append(character);
            }
        }

        return folded.ToString().Normalize(NormalizationForm.FormC);
    }
}
