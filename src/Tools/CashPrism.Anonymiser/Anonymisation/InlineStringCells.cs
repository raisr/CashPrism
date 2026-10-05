using System.Text.RegularExpressions;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Reads and rewrites the one cell shape this tool ever touches:
/// <c>&lt;c r="G2" t="inlineStr"&gt;&lt;is&gt;&lt;t&gt;…&lt;/t&gt;&lt;/is&gt;&lt;/c&gt;&gt;</c>. Measured on a
/// real export, this is the only attribute order and the only wrapper FinanzGuru
/// (via Apache POI) ever emits for a text cell; a blank cell is
/// <c>&lt;c r="H2"&gt;&lt;/c&gt;</c> with no <c>t</c> attribute at all, so it never matches
/// and is left untouched by construction — "an empty cell stays empty" needs no
/// special case.
/// </summary>
/// <remarks>
/// The captured text is the raw, still-XML-escaped substring between
/// <c>&lt;t&gt;</c> and <c>&lt;/t&gt;</c>. It is never unescaped: two different cell
/// values always escape to two different substrings when written by the same
/// encoder, so the raw text is a perfectly good dictionary key on its own, and
/// skipping unescape/re-escape avoids re-encoding a value neither this reader
/// nor its caller ever needs to interpret.
/// </remarks>
public static class InlineStringCells
{
    private static readonly Regex Pattern = new(
        """<c r="(?<letter>[A-Z]+)(?<row>[0-9]+)" t="inlineStr"><is><t>(?<text>[^<]*)</t></is></c>""",
        RegexOptions.Compiled);

    /// <summary>
    /// Resolves every <see cref="Infrastructure.Finanzguru.FinanzguruColumns"/>
    /// name to the spreadsheet column letter its header cell carries in row 1.
    /// Reading the letter from the header rather than computing it from a
    /// position keeps this independent of column order, the same guarantee
    /// <c>FinanzguruColumnMap</c> gives by resolving columns by name.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ResolveColumnLetters(string worksheetXml)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);

        var known = new HashSet<string>(Infrastructure.Finanzguru.FinanzguruColumns.All, StringComparer.Ordinal);
        var letters = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (Match match in Pattern.Matches(worksheetXml))
        {
            if (match.Groups["row"].Value != "1")
            {
                continue;
            }

            var header = match.Groups["text"].Value.Trim();

            if (known.Contains(header))
            {
                letters[header] = match.Groups["letter"].Value;
            }
        }

        return letters;
    }

    /// <summary>
    /// The raw text of every non-empty data-row cell (row 2 and below) sitting
    /// in one of <paramref name="columnLetters"/>, keyed by that letter, in the
    /// order the cells appear in the worksheet.
    /// </summary>
    public static IReadOnlyDictionary<string, List<string>> CollectValues(
        string worksheetXml, IReadOnlySet<string> columnLetters)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);

        var values = columnLetters.ToDictionary(letter => letter, _ => new List<string>(), StringComparer.Ordinal);

        foreach (Match match in Pattern.Matches(worksheetXml))
        {
            var letter = match.Groups["letter"].Value;

            if (match.Groups["row"].Value == "1" || !values.TryGetValue(letter, out var list))
            {
                continue;
            }

            list.Add(match.Groups["text"].Value);
        }

        return values;
    }

    /// <summary>
    /// Rewrites every data-row cell in one of <paramref name="columnLetters"/> by
    /// passing its raw text through <paramref name="replace"/>. A cell whose
    /// letter is not in <paramref name="columnLetters"/>, and the header row, are
    /// copied through unmatched — the regex simply does not touch them, which is
    /// what keeps every other byte of the worksheet identical to the input.
    /// </summary>
    public static string Rewrite(
        string worksheetXml,
        IReadOnlySet<string> columnLetters,
        Func<string /* columnLetter */, string /* rawText */, string> replace)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);
        ArgumentNullException.ThrowIfNull(replace);

        return Pattern.Replace(worksheetXml, match =>
        {
            var letter = match.Groups["letter"].Value;

            if (match.Groups["row"].Value == "1" || !columnLetters.Contains(letter))
            {
                return match.Value;
            }

            var replacement = replace(letter, match.Groups["text"].Value);

            return $"""<c r="{letter}{match.Groups["row"].Value}" t="inlineStr"><is><t>{XmlEscape(replacement)}</t></is></c>""";
        });
    }

    private static string XmlEscape(string value)
        => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
