using System.Globalization;
using System.Text.RegularExpressions;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Reads and rewrites the other cell shape this tool touches: a plain numeric
/// cell, <c>&lt;c r="D2" s="2"&gt;&lt;v&gt;123.45&lt;/v&gt;&lt;/c&gt;</c> — the shape FinanzGuru
/// (via Apache POI) uses for the two money columns, see
/// <c>docs/finanzguru-export.md</c>. Unlike <see cref="InlineStringCells"/>,
/// this tool never reads a numeric cell's value except to scale it, so there is
/// no counterpart to <c>CollectValues</c> here.
/// </summary>
public static class NumericCells
{
    private static readonly Regex Pattern = new(
        """<c r="(?<letter>[A-Z]+)(?<row>[0-9]+)"(?<attributes>[^>]*)><v>(?<value>[^<]*)</v></c>""",
        RegexOptions.Compiled);

    /// <summary>
    /// Rewrites every data-row cell in one of <paramref name="columnLetters"/> by
    /// passing its numeric value through <paramref name="transform"/>. A cell
    /// whose letter is not in <paramref name="columnLetters"/>, and the header
    /// row, are copied through unmatched.
    /// </summary>
    public static string Rewrite(
        string worksheetXml,
        IReadOnlySet<string> columnLetters,
        Func<decimal, decimal> transform)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);
        ArgumentNullException.ThrowIfNull(transform);

        return Pattern.Replace(worksheetXml, match =>
        {
            var letter = match.Groups["letter"].Value;

            if (match.Groups["row"].Value == "1" || !columnLetters.Contains(letter))
            {
                return match.Value;
            }

            var value = decimal.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
            var replacement = transform(value);

            return $"""<c r="{letter}{match.Groups["row"].Value}"{match.Groups["attributes"].Value}><v>{replacement.ToString("0.00", CultureInfo.InvariantCulture)}</v></c>""";
        });
    }
}
