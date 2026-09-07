using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Produces the anonymised worksheet XML for one file: every cell in a replaced
/// column gets its dictionary replacement, <c>Betrag</c> and <c>Kontostand</c>
/// are scaled together, every other byte is untouched.
/// </summary>
public static class WorksheetAnonymiser
{
    /// <summary>The columns <see cref="Rewrite"/> scales instead of replacing.</summary>
    public static IReadOnlySet<string> ScaledColumns { get; } =
        new HashSet<string>([FinanzguruColumns.Amount, FinanzguruColumns.Balance], StringComparer.Ordinal);

    /// <summary>
    /// Rewrites <paramref name="worksheetXml"/> using <paramref name="dictionaries"/>.
    /// </summary>
    /// <param name="columnLetters">This file's own column name to letter mapping, as read from its header row.</param>
    /// <param name="scale">
    /// The factor <see cref="ScaledColumns"/> are multiplied by, rounded to two
    /// decimals. <c>1.0</c> leaves them byte-identical to the input — no cell
    /// is even touched.
    /// </param>
    public static string Rewrite(
        string worksheetXml,
        IReadOnlyDictionary<string, string> columnLetters,
        AnonymisationDictionaries dictionaries,
        decimal scale)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);
        ArgumentNullException.ThrowIfNull(dictionaries);

        var columnNameByLetter = ColumnNameByReplacedLetter(columnLetters);

        var replaced = InlineStringCells.Rewrite(
            worksheetXml,
            (IReadOnlySet<string>)columnNameByLetter.Keys.ToHashSet(StringComparer.Ordinal),
            (letter, rawText) => dictionaries.Replace(columnNameByLetter[letter], rawText));

        if (scale == 1.0m)
        {
            return replaced;
        }

        var scaledLetters = ScaledColumns
            .Select(column => columnLetters[column])
            .ToHashSet(StringComparer.Ordinal);

        return NumericCells.Rewrite(
            replaced, scaledLetters, value => Math.Round(value * scale, 2, MidpointRounding.AwayFromZero));
    }

    private static Dictionary<string, string> ColumnNameByReplacedLetter(
        IReadOnlyDictionary<string, string> columnLetters)
        => columnLetters
            .Where(pair => AnonymisationDictionaries.ReplacedColumns.Contains(pair.Key))
            .ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);
}
