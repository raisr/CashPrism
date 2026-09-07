using System.Text.RegularExpressions;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Keeps the header row plus the newest <c>n</c> data rows of a worksheet part,
/// dropping the rest — a plain prefix, because a FinanzGuru export's row order
/// is already newest first. Applied before any value is read out of the
/// worksheet, so a value that only occurred in a dropped row never claims a
/// dictionary entry.
/// </summary>
public static class WorksheetRowLimiter
{
    private static readonly Regex RowPattern = new("<row\\b.*?</row>", RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Trims <paramref name="worksheetXml"/> to its header row plus the first
    /// <paramref name="maxRows"/> data rows. Returns the worksheet unchanged,
    /// together with <paramref name="dataRowCount"/>, when
    /// <paramref name="maxRows"/> is <see langword="null"/> or already covers
    /// every data row.
    /// </summary>
    /// <param name="dataRowCount">
    /// The number of data rows <paramref name="worksheetXml"/> carries, as
    /// already established by <see cref="WorksheetValidation"/>.
    /// </param>
    public static (string WorksheetXml, int DataRowCount) Limit(string worksheetXml, int? maxRows, int dataRowCount)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);

        if (maxRows is null || maxRows.Value >= dataRowCount)
        {
            return (worksheetXml, dataRowCount);
        }

        var rows = RowPattern.Matches(worksheetXml);
        var lastKeptRow = rows[maxRows.Value]; // rows[0] is the header row.
        var lastRow = rows[^1];

        var trimmed = worksheetXml[..(lastKeptRow.Index + lastKeptRow.Length)]
            + worksheetXml[(lastRow.Index + lastRow.Length)..];

        return (trimmed, maxRows.Value);
    }
}
