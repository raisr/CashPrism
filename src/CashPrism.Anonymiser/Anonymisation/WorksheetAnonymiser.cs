namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Produces the anonymised worksheet XML for one file: every cell in a replaced
/// column gets its dictionary replacement, every other byte is untouched.
/// </summary>
public static class WorksheetAnonymiser
{
    /// <summary>
    /// Rewrites <paramref name="worksheetXml"/> using <paramref name="dictionaries"/>.
    /// </summary>
    /// <param name="columnLetters">This file's own column name to letter mapping, as read from its header row.</param>
    public static string Rewrite(
        string worksheetXml,
        IReadOnlyDictionary<string, string> columnLetters,
        AnonymisationDictionaries dictionaries)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);
        ArgumentNullException.ThrowIfNull(dictionaries);

        var columnNameByLetter = ColumnNameByReplacedLetter(columnLetters);

        return InlineStringCells.Rewrite(
            worksheetXml,
            (IReadOnlySet<string>)columnNameByLetter.Keys.ToHashSet(StringComparer.Ordinal),
            (letter, rawText) => dictionaries.Replace(columnNameByLetter[letter], rawText));
    }

    private static Dictionary<string, string> ColumnNameByReplacedLetter(
        IReadOnlyDictionary<string, string> columnLetters)
        => columnLetters
            .Where(pair => AnonymisationDictionaries.ReplacedColumns.Contains(pair.Key))
            .ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);
}
