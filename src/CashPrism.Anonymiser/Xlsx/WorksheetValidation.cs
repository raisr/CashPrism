using System.Xml.Linq;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Checks a worksheet part against the shapes this tool can safely copy
/// through: inline strings only, and a header row that resolves against
/// <see cref="FinanzguruColumnMap"/>. Reading is not writing — nothing here
/// changes a byte, it only decides whether <see cref="XlsxRoundTrip"/> may
/// proceed.
/// </summary>
public static class WorksheetValidation
{
    private static readonly XNamespace Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    /// <summary>Validates the worksheet XML read from the located entry.</summary>
    public static WorksheetValidationResult Validate(string worksheetXml)
    {
        ArgumentNullException.ThrowIfNull(worksheetXml);

        var document = XDocument.Parse(worksheetXml);
        var rows = document.Descendants(Namespace + "sheetData").Elements(Namespace + "row").ToArray();

        if (rows.Length == 0)
        {
            return WorksheetValidationResult.Failure("The worksheet has no rows.");
        }

        var cells = rows.SelectMany(row => row.Elements(Namespace + "c")).ToArray();

        if (cells.Any(cell => (string?)cell.Attribute("t") == "s"))
        {
            return WorksheetValidationResult.Failure(
                "The worksheet uses shared strings (t=\"s\"), which this tool does not support.");
        }

        var headerRow = rows[0]
            .Elements(Namespace + "c")
            .Select(ReadInlineStringValue)
            .Where(value => !string.IsNullOrEmpty(value))
            .ToArray();

        var columnMap = FinanzguruColumnMap.Resolve(headerRow!);

        if (columnMap.MissingColumns.Count > 0)
        {
            return WorksheetValidationResult.Failure(
                $"The worksheet is missing the column(s): {string.Join(", ", columnMap.MissingColumns)}.");
        }

        if (columnMap.DuplicateColumns.Count > 0)
        {
            return WorksheetValidationResult.Failure(
                $"The worksheet carries the column(s) more than once: {string.Join(", ", columnMap.DuplicateColumns)}.");
        }

        if (columnMap.UnknownColumns.Count > 0)
        {
            return WorksheetValidationResult.Failure(
                $"The worksheet carries unknown column(s): {string.Join(", ", columnMap.UnknownColumns)}.");
        }

        return WorksheetValidationResult.Success(rows.Length - 1);
    }

    private static string? ReadInlineStringValue(XElement cell)
        => (string?)cell.Element(Namespace + "is")?.Element(Namespace + "t");
}
