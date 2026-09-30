using System.Globalization;
using System.Text.RegularExpressions;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// The counterpart of <see cref="WorksheetSelfCheck"/> for
/// <see cref="SyntheticValues"/>: reads the written worksheet back and compares
/// every date and amount with the cell it replaced. Generated values can
/// coincide with real ones by accident, and a file that claims to carry no real
/// value must have that checked, not assumed.
/// </summary>
public static class SyntheticValuesSelfCheck
{
    /// <summary>The columns whose every non-zero cell has to differ from the input.</summary>
    public static IReadOnlyList<string> CheckedColumns { get; } =
        [FinanzguruColumns.BookingDate, FinanzguruColumns.Amount, FinanzguruColumns.Balance];

    private static readonly Regex NumericCell = new(
        """<c r="(?<reference>(?<letter>[A-Z]+)(?<row>[0-9]+))"[^>]*><v>(?<value>[^<]*)</v></c>""",
        RegexOptions.Compiled);

    /// <summary>
    /// The checked columns, if any, in which a data-row cell of
    /// <paramref name="outputWorksheetXml"/> still carries the value the same cell
    /// had in <paramref name="inputWorksheetXml"/>. Values are compared as
    /// numbers, since the two may print them differently. A zero is exempt: it
    /// stays zero on purpose and says nothing about anybody.
    /// </summary>
    /// <param name="inputWorksheetXml">The worksheet as it was read, after any row limit.</param>
    /// <param name="outputWorksheetXml">The worksheet as it was written.</param>
    /// <param name="columnLetters">This file's own column name to letter mapping.</param>
    public static IReadOnlyList<string> FindKeptColumns(
        string inputWorksheetXml,
        string outputWorksheetXml,
        IReadOnlyDictionary<string, string> columnLetters)
    {
        ArgumentNullException.ThrowIfNull(inputWorksheetXml);
        ArgumentNullException.ThrowIfNull(outputWorksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);

        var columnByLetter = CheckedColumns
            .Where(columnLetters.ContainsKey)
            .ToDictionary(column => columnLetters[column], column => column, StringComparer.Ordinal);

        var input = Values(inputWorksheetXml, columnByLetter);
        var output = Values(outputWorksheetXml, columnByLetter);

        return output
            .Where(cell => cell.Value != 0m
                && input.TryGetValue(cell.Key, out var original)
                && original == cell.Value)
            .Select(cell => columnByLetter[Letter(cell.Key)])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static Dictionary<string, decimal> Values(string worksheetXml, IReadOnlyDictionary<string, string> columnByLetter)
    {
        var values = new Dictionary<string, decimal>(StringComparer.Ordinal);

        foreach (Match match in NumericCell.Matches(worksheetXml))
        {
            if (match.Groups["row"].Value != "1" && columnByLetter.ContainsKey(match.Groups["letter"].Value))
            {
                values[match.Groups["reference"].Value] = decimal.Parse(
                    match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }

        return values;
    }

    private static string Letter(string reference) => reference.TrimEnd("0123456789".ToCharArray());
}
