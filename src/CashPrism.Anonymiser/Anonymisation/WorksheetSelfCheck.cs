namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Reads an anonymised worksheet back and checks the one thing that matters
/// more than anything else this tool does: that no replaced column still holds
/// one of its original values. A file with forgotten cleartext looks exactly
/// like a finished one otherwise — this is the only mechanism that turns "we
/// replaced it" into a checked fact.
/// </summary>
public static class WorksheetSelfCheck
{
    /// <summary>
    /// The replaced columns, if any, whose output still carries at least one of
    /// their original values. Empty means the file is clean.
    /// </summary>
    public static IReadOnlyList<string> FindLeakedColumns(
        string outputWorksheetXml,
        IReadOnlyDictionary<string, string> columnLetters,
        AnonymisationDictionaries dictionaries)
    {
        ArgumentNullException.ThrowIfNull(outputWorksheetXml);
        ArgumentNullException.ThrowIfNull(columnLetters);
        ArgumentNullException.ThrowIfNull(dictionaries);

        var replacedColumns = columnLetters
            .Where(pair => AnonymisationDictionaries.ReplacedColumns.Contains(pair.Key))
            .ToList();

        var letters = replacedColumns.Select(pair => pair.Value).ToHashSet(StringComparer.Ordinal);
        var outputValuesByLetter = InlineStringCells.CollectValues(outputWorksheetXml, letters);

        var leaked = new List<string>();

        foreach (var (columnName, letter) in replacedColumns)
        {
            var originalValues = dictionaries.OriginalValues(columnName);

            if (outputValuesByLetter.TryGetValue(letter, out var outputValues)
                && outputValues.Any(originalValues.Contains))
            {
                leaked.Add(columnName);
            }
        }

        return leaked;
    }
}
