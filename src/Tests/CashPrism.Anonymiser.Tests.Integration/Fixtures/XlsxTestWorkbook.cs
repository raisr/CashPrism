using System.Globalization;
using System.IO.Compression;
using System.Text;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Integration.Fixtures;

/// <summary>
/// Builds a minimal, hand-written <c>.xlsx</c> in memory: inline strings
/// throughout, an otherwise-empty <c>sharedStrings.xml</c>, and two numeric
/// formats — <c>numFmt 164</c> (<c>dd.MM.yyyy</c>) on
/// <see cref="FinanzguruColumns.BookingDate"/> and the built-in
/// <c>#,##0.00</c> on <see cref="FinanzguruColumns.Amount"/> and
/// <see cref="FinanzguruColumns.Balance"/> — the same quirks
/// <c>docs/finanzguru-export.md</c> records for a real export. No binary
/// fixture enters the repository; every byte the round-trip test compares
/// against is produced here, in code.
/// </summary>
internal static class XlsxTestWorkbook
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string DocumentRelationshipsNamespace =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static readonly DateOnly ExcelEpoch = new(1899, 12, 30);
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Builds the workbook. Every data row maps column name to its text value;
    /// a value under <see cref="FinanzguruColumns.BookingDate"/> is written as a
    /// styled numeric date cell (<c>dd.MM.yyyy</c>) rather than an inline
    /// string, and a missing key or an empty value leaves the cell out
    /// entirely, matching how FinanzGuru itself leaves a booking's unused
    /// columns blank.
    /// </summary>
    public static byte[] Build(
        IReadOnlyList<string> headerNames,
        IReadOnlyList<IReadOnlyDictionary<string, string>> dataRows,
        bool useSharedStrings = false)
    {
        ArgumentNullException.ThrowIfNull(headerNames);
        ArgumentNullException.ThrowIfNull(dataRows);

        var sharedStrings = new List<string>();
        var worksheetXml = BuildWorksheetXml(headerNames, dataRows, useSharedStrings, sharedStrings);

        using var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", ContentTypesXml);
            WriteEntry(archive, "_rels/.rels", PackageRelationshipsXml);
            WriteEntry(archive, "xl/workbook.xml", WorkbookXml);
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelationshipsXml);
            WriteEntry(archive, "xl/styles.xml", StylesXml);
            WriteEntry(archive, "xl/sharedStrings.xml", BuildSharedStringsXml(sharedStrings));
            WriteEntry(archive, "xl/worksheets/sheet1.xml", worksheetXml);
        }

        return stream.ToArray();
    }

    private static string BuildWorksheetXml(
        IReadOnlyList<string> headerNames,
        IReadOnlyList<IReadOnlyDictionary<string, string>> dataRows,
        bool useSharedStrings,
        List<string> sharedStrings)
    {
        var builder = new StringBuilder();

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        builder.Append($"<worksheet xmlns=\"{SpreadsheetNamespace}\"><sheetData>");

        builder.Append("<row r=\"1\">");
        for (var column = 0; column < headerNames.Count; column++)
        {
            AppendStringCell(builder, column, 1, headerNames[column], useSharedStrings, sharedStrings);
        }

        builder.Append("</row>");

        for (var rowIndex = 0; rowIndex < dataRows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 2;
            var row = dataRows[rowIndex];

            builder.Append($"<row r=\"{rowNumber}\">");

            for (var column = 0; column < headerNames.Count; column++)
            {
                if (!row.TryGetValue(headerNames[column], out var value) || value.Length == 0)
                {
                    continue;
                }

                if (headerNames[column] == FinanzguruColumns.BookingDate)
                {
                    AppendDateCell(builder, column, rowNumber, value);
                }
                else if (headerNames[column] is FinanzguruColumns.Amount or FinanzguruColumns.Balance)
                {
                    AppendMoneyCell(builder, column, rowNumber, value);
                }
                else
                {
                    AppendStringCell(builder, column, rowNumber, value, useSharedStrings, sharedStrings);
                }
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData></worksheet>");

        return builder.ToString();
    }

    private static void AppendStringCell(
        StringBuilder builder,
        int columnIndex,
        int rowNumber,
        string value,
        bool useSharedStrings,
        List<string> sharedStrings)
    {
        var reference = $"{ColumnLetters(columnIndex)}{rowNumber}";

        if (useSharedStrings)
        {
            var sharedIndex = sharedStrings.IndexOf(value);

            if (sharedIndex < 0)
            {
                sharedIndex = sharedStrings.Count;
                sharedStrings.Add(value);
            }

            builder.Append($"<c r=\"{reference}\" t=\"s\"><v>{sharedIndex}</v></c>");
        }
        else
        {
            builder.Append($"<c r=\"{reference}\" t=\"inlineStr\"><is><t>{XmlEscape(value)}</t></is></c>");
        }
    }

    private static void AppendDateCell(StringBuilder builder, int columnIndex, int rowNumber, string value)
    {
        var reference = $"{ColumnLetters(columnIndex)}{rowNumber}";
        var date = DateOnly.ParseExact(value, "dd.MM.yyyy");
        var serial = date.DayNumber - ExcelEpoch.DayNumber;

        builder.Append($"<c r=\"{reference}\" s=\"1\"><v>{serial}</v></c>");
    }

    private static void AppendMoneyCell(StringBuilder builder, int columnIndex, int rowNumber, string value)
    {
        var reference = $"{ColumnLetters(columnIndex)}{rowNumber}";
        var amount = decimal.Parse(value, CultureInfo.InvariantCulture);

        builder.Append($"<c r=\"{reference}\" s=\"2\"><v>{amount.ToString("0.00", CultureInfo.InvariantCulture)}</v></c>");
    }

    private static string BuildSharedStringsXml(IReadOnlyList<string> sharedStrings)
    {
        var builder = new StringBuilder();

        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        builder.Append(
            $"<sst xmlns=\"{SpreadsheetNamespace}\" count=\"{sharedStrings.Count}\" uniqueCount=\"{sharedStrings.Count}\">");

        foreach (var value in sharedStrings)
        {
            builder.Append($"<si><t>{XmlEscape(value)}</t></si>");
        }

        builder.Append("</sst>");

        return builder.ToString();
    }

    private static string ColumnLetters(int columnIndex)
    {
        var letters = new StringBuilder();
        var value = columnIndex;

        do
        {
            letters.Insert(0, (char)('A' + (value % 26)));
            value = (value / 26) - 1;
        }
        while (value >= 0);

        return letters.ToString();
    }

    private static string XmlEscape(string value)
        => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static void WriteEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Utf8NoBom);

        writer.Write(content);
    }

    private static string ContentTypesXml =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
        + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
        + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
        + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
        + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
        + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>"
        + "<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>"
        + "</Types>";

    private static string PackageRelationshipsXml =>
        $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + $"<Relationships xmlns=\"{RelationshipsNamespace}\">"
        + $"<Relationship Id=\"rId1\" Type=\"{DocumentRelationshipsNamespace}/officeDocument\" Target=\"xl/workbook.xml\"/>"
        + "</Relationships>";

    private static string WorkbookXml =>
        $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + $"<workbook xmlns=\"{SpreadsheetNamespace}\" xmlns:r=\"{DocumentRelationshipsNamespace}\">"
        + "<sheets><sheet name=\"20260907_Export_Alle_Buchungen\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
        + "</workbook>";

    private static string WorkbookRelationshipsXml =>
        $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + $"<Relationships xmlns=\"{RelationshipsNamespace}\">"
        + $"<Relationship Id=\"rId1\" Type=\"{DocumentRelationshipsNamespace}/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
        + $"<Relationship Id=\"rId2\" Type=\"{DocumentRelationshipsNamespace}/styles\" Target=\"styles.xml\"/>"
        + $"<Relationship Id=\"rId3\" Type=\"{DocumentRelationshipsNamespace}/sharedStrings\" Target=\"sharedStrings.xml\"/>"
        + "</Relationships>";

    private static string StylesXml =>
        $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + $"<styleSheet xmlns=\"{SpreadsheetNamespace}\">"
        + "<numFmts count=\"1\"><numFmt numFmtId=\"164\" formatCode=\"dd.MM.yyyy\"/></numFmts>"
        + "<fonts count=\"1\"><font/></fonts>"
        + "<fills count=\"1\"><fill/></fills>"
        + "<borders count=\"1\"><border/></borders>"
        + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
        + "<cellXfs count=\"3\">"
        + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/>"
        + "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"4\" fontId=\"0\" fillId=\"0\" borderId=\"0\" applyNumberFormat=\"1\"/>"
        + "</cellXfs>"
        + "</styleSheet>";
}
