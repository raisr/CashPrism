using System.Globalization;
using System.Text.RegularExpressions;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Replaces the values the anonymiser otherwise keeps — <c>Buchungstag</c>,
/// <c>Betrag</c>, <c>Kontostand</c> and the four period columns derived from the
/// date — with generated ones, for a file that has to carry no value of the real
/// export at all.
/// </summary>
/// <remarks>
/// <para>
/// Generated, not transformed. The tool has no salt and nothing secret, so a
/// constant offset or factor would be undone by anyone who reads this file.
/// Every value here is computed from the row it sits in, never from the value it
/// replaces; the original only lends its sign and its time of day.
/// </para>
/// <para>
/// Only the text inside a replaced cell changes. Its reference, style index and
/// type attribute are copied through, so the output still reads as a Finanzguru
/// export — which is what a fixture built from it is for.
/// </para>
/// </remarks>
public static class SyntheticValues
{
    /// <summary>
    /// The date the first data row gets; each further row is one day earlier.
    /// Before Finanzguru existed, so no generated date can be a real one, and
    /// descending, so the export's newest-first order holds.
    /// </summary>
    public static readonly DateOnly FirstDate = new(2001, 1, 1);

    private static readonly DateOnly ExcelEpoch = new(1899, 12, 30);

    private static readonly Regex RowPattern = new(
        """<row\b[^>]*>.*?</row>""",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex RowNumber = new(
        """^<row\b[^>]*?\sr="(?<number>[0-9]+)""",
        RegexOptions.Compiled);

    private static readonly Regex NumericCell = new(
        """<c r="(?<letter>[A-Z]+)(?<row>[0-9]+)"(?<attributes>[^>]*)><v>(?<value>[^<]*)</v></c>""",
        RegexOptions.Compiled);

    private static readonly Regex InlineStringCell = new(
        """<c r="(?<letter>[A-Z]+)(?<row>[0-9]+)" t="inlineStr"><is><t>(?<text>[^<]*)</t></is></c>""",
        RegexOptions.Compiled);

    /// <summary>The columns <see cref="Rewrite"/> replaces, all of them kept by the anonymiser otherwise.</summary>
    public static IReadOnlySet<string> ReplacedColumns { get; } = new HashSet<string>(
        [
            FinanzguruColumns.BookingDate,
            FinanzguruColumns.Amount,
            FinanzguruColumns.Balance,
            FinanzguruColumns.Week,
            FinanzguruColumns.Month,
            FinanzguruColumns.Quarter,
            FinanzguruColumns.Year,
        ],
        StringComparer.Ordinal);

    /// <summary>
    /// Rewrites every data row of <paramref name="worksheetXml"/>. Has to run
    /// before the booking ids are replaced: a split booking is recognised by the
    /// ids its parts point back with.
    /// </summary>
    /// <param name="columnLetters">This file's own column name to letter mapping, as read from its header row.</param>
    public static string Rewrite(string worksheetXml, IReadOnlyDictionary<string, string> columnLetters)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);

        var letters = new Letters(columnLetters);
        var rows = RowPattern.Matches(worksheetXml)
            .Skip(1) // the header row
            .Select((match, index) => ReadRow(match.Value, index, letters))
            .ToList();

        var amounts = GenerateAmounts(rows);

        // The same rows again, in the same order: the first match is the header.
        var position = -1;

        return RowPattern.Replace(worksheetXml, match =>
        {
            position++;

            return position == 0 ? match.Value : RewriteRow(rows[position - 1], amounts, letters);
        });
    }

    private static Row ReadRow(string rowXml, int index, Letters letters)
    {
        string? Numeric(string? letter) => letter is null ? null : Cell(NumericCell, rowXml, letter, "value");
        string? Text(string? letter) => letter is null ? null : Cell(InlineStringCell, rowXml, letter, "text");

        var number = int.Parse(RowNumber.Match(rowXml).Groups["number"].Value, CultureInfo.InvariantCulture);

        return new Row(
            rowXml,
            index,
            number,
            Amount: Numeric(letters.Amount),
            BookingId: Text(letters.BookingId),
            OriginalReferenceId: Text(letters.OriginalReferenceId),
            SplitType: Text(letters.SplitType));
    }

    private static string? Cell(Regex pattern, string rowXml, string letter, string group)
    {
        foreach (Match match in pattern.Matches(rowXml))
        {
            if (match.Groups["letter"].Value == letter)
            {
                return match.Groups[group].Value;
            }
        }

        return null;
    }

    /// <summary>
    /// The generated <c>Betrag</c> of every row, by row number. The original of a
    /// split booking takes the sum of its parts that are in the file, so the
    /// invariant <c>docs/finanzguru-export.md</c> documents still holds.
    /// </summary>
    private static Dictionary<int, decimal?> GenerateAmounts(IReadOnlyList<Row> rows)
    {
        var amounts = rows.ToDictionary(
            row => row.Number,
            row => row.Amount is null ? (decimal?)null : Generate(row.Amount, row.Number, multiplier: 7_919, modulus: 99_999));

        var partsByOriginal = rows
            .Where(row => row.SplitType is FinanzguruSplitType.Part or FinanzguruSplitType.Remainder
                && !string.IsNullOrEmpty(row.OriginalReferenceId))
            .GroupBy(row => row.OriginalReferenceId!, StringComparer.Ordinal);

        foreach (var parts in partsByOriginal)
        {
            var original = rows.FirstOrDefault(row =>
                row.SplitType == FinanzguruSplitType.Original
                && string.Equals(row.BookingId, parts.Key, StringComparison.Ordinal));

            if (original?.Amount is not null)
            {
                amounts[original.Number] = parts.Sum(part => amounts[part.Number] ?? 0m);
            }
        }

        return amounts;
    }

    /// <summary>
    /// A value built from the row number alone, carrying the original's sign. A
    /// zero stays zero: it says nothing about anybody, and giving it a sign would
    /// contradict the row's own <c>Analyse-Betrag</c>. Should the generated value
    /// happen to equal the original, it moves by a cent.
    /// </summary>
    private static decimal Generate(string originalText, int rowNumber, long multiplier, long modulus)
    {
        var original = decimal.Parse(originalText, NumberStyles.Float, CultureInfo.InvariantCulture);

        if (original == 0m)
        {
            return 0m;
        }

        var cents = ((rowNumber * multiplier) % modulus) + 1;

        if (cents == Math.Abs(original * 100m))
        {
            cents++;
        }

        return Math.Sign(original) * cents / 100m;
    }

    private static string RewriteRow(Row row, IReadOnlyDictionary<int, decimal?> amounts, Letters letters)
    {
        var amount = amounts[row.Number];
        var date = FirstDate.AddDays(-row.Index);

        var xml = NumericCell.Replace(row.Xml, match =>
        {
            var letter = match.Groups["letter"].Value;
            var value = match.Groups["value"].Value;

            string? replacement = letter switch
            {
                _ when letter == letters.BookingDate => DateText(date, value),
                _ when letter == letters.Amount && amount is not null => NumberText(amount.Value),
                _ when letter == letters.Balance => NumberText(Generate(value, row.Number, multiplier: 104_729, modulus: 9_999_999)),
                _ when letter == letters.Year => FinanzguruPeriodLabels.Year(date),
                _ => null,
            };

            return replacement is null
                ? match.Value
                : $"""<c r="{letter}{match.Groups["row"].Value}"{match.Groups["attributes"].Value}><v>{replacement}</v></c>""";
        });

        return InlineStringCell.Replace(xml, match =>
        {
            var letter = match.Groups["letter"].Value;

            string? replacement = letter switch
            {
                _ when letter == letters.Week => FinanzguruPeriodLabels.Week(date),
                _ when letter == letters.Month => FinanzguruPeriodLabels.Month(date),
                _ when letter == letters.Quarter => FinanzguruPeriodLabels.Quarter(date),
                _ => null,
            };

            return replacement is null
                ? match.Value
                : $"""<c r="{letter}{match.Groups["row"].Value}" t="inlineStr"><is><t>{replacement}</t></is></c>""";
        });
    }

    /// <summary>
    /// The date as a serial number, keeping the original's fractional part digit
    /// for digit: the time of day of the few rows that have one is what makes
    /// them a case worth having in a fixture, and it identifies nothing.
    /// </summary>
    private static string DateText(DateOnly date, string originalText)
    {
        var serial = (date.DayNumber - ExcelEpoch.DayNumber).ToString(CultureInfo.InvariantCulture);
        var point = originalText.IndexOf('.', StringComparison.Ordinal);

        return point < 0 ? serial : serial + originalText[point..];
    }

    /// <summary>
    /// A number the way the export writes it — Java's double printing: no
    /// trailing zeros, and a whole number with <c>.0</c>.
    /// </summary>
    private static string NumberText(decimal value)
    {
        var text = value.ToString("0.##", CultureInfo.InvariantCulture);

        return text.Contains('.', StringComparison.Ordinal) ? text : text + ".0";
    }

    private sealed record Row(
        string Xml,
        int Index,
        int Number,
        string? Amount,
        string? BookingId,
        string? OriginalReferenceId,
        string? SplitType);

    private sealed class Letters(IReadOnlyDictionary<string, string> columnLetters)
    {
        public string? BookingDate { get; } = Find(columnLetters, FinanzguruColumns.BookingDate);

        public string? Amount { get; } = Find(columnLetters, FinanzguruColumns.Amount);

        public string? Balance { get; } = Find(columnLetters, FinanzguruColumns.Balance);

        public string? Week { get; } = Find(columnLetters, FinanzguruColumns.Week);

        public string? Month { get; } = Find(columnLetters, FinanzguruColumns.Month);

        public string? Quarter { get; } = Find(columnLetters, FinanzguruColumns.Quarter);

        public string? Year { get; } = Find(columnLetters, FinanzguruColumns.Year);

        public string? BookingId { get; } = Find(columnLetters, FinanzguruColumns.BookingId);

        public string? OriginalReferenceId { get; } = Find(columnLetters, FinanzguruColumns.OriginalReferenceId);

        public string? SplitType { get; } = Find(columnLetters, FinanzguruColumns.SplitType);

        private static string? Find(IReadOnlyDictionary<string, string> columnLetters, string column)
            => columnLetters.TryGetValue(column, out var letter) ? letter : null;
    }
}
