namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Resolves a FinanzGuru header row to the column index of every known column.
/// </summary>
/// <remarks>
/// <para>
/// Columns are resolved by header name, never by position. The export comes from
/// a third party we do not control, and a column silently shifting by one would
/// make a writer overwrite the wrong column — the most expensive way a tool like
/// this can fail. A missing column therefore fails loudly rather than being
/// tolerated.
/// </para>
/// <para>
/// The input is a plain list of strings and the output a plain assignment, so
/// this works without a workbook: the anonymiser takes no ClosedXML dependency,
/// and the parser reuses the same rules.
/// </para>
/// </remarks>
public static class FinanzguruColumnMap
{
    /// <summary>
    /// Assigns every column in <see cref="FinanzguruColumns.All"/> the index it
    /// sits at in <paramref name="headerRow"/>. Header names are compared
    /// ordinally after trimming surrounding whitespace; blank cells are ignored,
    /// so the trailing empty cells a spreadsheet tends to carry are not reported
    /// as unknown columns.
    /// </summary>
    /// <param name="headerRow">
    /// The first row of the worksheet, one entry per column, in sheet order. The
    /// order itself does not matter — only the names do.
    /// </param>
    /// <returns>
    /// A successful result carrying the assignment, or a failure naming the
    /// columns that are missing or ambiguous. See
    /// <see cref="FinanzguruColumnMapResult"/>.
    /// </returns>
    public static FinanzguruColumnMapResult Resolve(IReadOnlyList<string> headerRow)
    {
        ArgumentNullException.ThrowIfNull(headerRow);

        var known = new HashSet<string>(FinanzguruColumns.All, StringComparer.Ordinal);
        var columns = new Dictionary<string, int>(StringComparer.Ordinal);
        var unknownColumns = new List<string>();
        var duplicateColumns = new List<string>();

        for (var index = 0; index < headerRow.Count; index++)
        {
            var header = headerRow[index]?.Trim();

            if (string.IsNullOrEmpty(header))
            {
                continue;
            }

            if (!known.Contains(header))
            {
                AddOnce(unknownColumns, header);
                continue;
            }

            if (!columns.TryAdd(header, index))
            {
                AddOnce(duplicateColumns, header);
            }
        }

        var missingColumns = FinanzguruColumns.All
            .Where(column => !columns.ContainsKey(column))
            .ToArray();

        if (missingColumns.Length > 0 || duplicateColumns.Count > 0)
        {
            return new FinanzguruColumnMapResult(
                Columns: new Dictionary<string, int>(StringComparer.Ordinal),
                MissingColumns: missingColumns,
                UnknownColumns: unknownColumns,
                DuplicateColumns: duplicateColumns);
        }

        return new FinanzguruColumnMapResult(
            Columns: columns,
            MissingColumns: [],
            UnknownColumns: unknownColumns,
            DuplicateColumns: []);
    }

    private static void AddOnce(List<string> names, string name)
    {
        if (!names.Contains(name, StringComparer.Ordinal))
        {
            names.Add(name);
        }
    }
}
