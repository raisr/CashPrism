using System.Globalization;
using ClosedXML.Excel;

namespace CashPrism.DemoData.Tests.Integration.Xlsx;

/// <summary>
/// What a workbook says, as opposed to how its bytes are laid out: every
/// worksheet's name and every used cell's address, type and value. Two files
/// written a second apart differ in their zip timestamps and agree here.
/// </summary>
internal static class WorkbookContent
{
    /// <summary>One line per worksheet name and one per non-empty cell, in sheet order.</summary>
    public static IReadOnlyList<string> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var lines = new List<string>();

        foreach (var sheet in workbook.Worksheets)
        {
            lines.Add("sheet " + sheet.Name);
            lines.AddRange(sheet.CellsUsed().Select(cell => string.Join(
                ' ',
                cell.Address.ToString(),
                cell.DataType,
                cell.Value.ToString(CultureInfo.InvariantCulture))));
        }

        return lines;
    }

    /// <inheritdoc cref="Read(Stream)"/>
    public static IReadOnlyList<string> Read(string path)
    {
        using var stream = File.OpenRead(path);

        return Read(stream);
    }
}
