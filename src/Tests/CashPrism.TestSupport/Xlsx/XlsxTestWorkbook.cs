using System.Globalization;
using System.IO.Compression;
using System.Text;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.TestSupport.Xlsx;

/// <summary>
/// Builds a minimal, hand-written <c>.xlsx</c> in memory: inline strings
/// throughout, an otherwise-empty <c>sharedStrings.xml</c>, and two numeric
/// formats — <c>numFmt 164</c> (<c>dd.MM.yyyy</c>) on
/// <see cref="FinanzguruColumns.BookingDate"/> and the built-in
/// <c>#,##0.00</c> on <see cref="FinanzguruColumns.Amount"/> and
/// <see cref="FinanzguruColumns.Balance"/> — the same quirks
/// <c>docs/finanzguru-export.md</c> records for a real export. No binary
/// fixture enters the repository; every byte a test reads is produced here, in
/// code.
/// </summary>
public static class XlsxTestWorkbook
{
    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string DocumentRelationshipsNamespace =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    /// <summary>
    /// The worksheet name a built workbook carries unless a test asks for
    /// another. Shaped like a real export's, so the export date reads back.
    /// </summary>
    public const string DefaultSheetName = "20260907_Export_Alle_Buchungen";

    /// <summary>
    /// The shape a <see cref="FinanzguruColumns.BookingDate"/> value may take to
    /// ask for a time component. A plain <c>dd.MM.yyyy</c> writes a whole-day
    /// serial; this one writes a fractional serial, which is what the PayPal
    /// rows of a real export carry.
    /// </summary>
    public const string DateTimeValueFormat = "dd.MM.yyyy HH:mm:ss";

    private static readonly DateTime ExcelEpoch = new(1899, 12, 30, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Builds the workbook. Every data row maps column name to its text value;
    /// a value under <see cref="FinanzguruColumns.BookingDate"/> is written as a
    /// styled numeric date cell (<c>dd.MM.yyyy</c>) rather than an inline
    /// string, and a missing key or an empty value leaves the cell out
    /// entirely, matching how FinanzGuru itself leaves a booking's unused
    /// columns blank.
    /// </summary>
    /// <param name="headerNames">The header row, one entry per column, in sheet order.</param>
    /// <param name="dataRows">The data rows, each mapping a header name to its text value.</param>
    /// <param name="useSharedStrings">
    /// Whether to put strings in <c>sharedStrings.xml</c> instead of inline.
    /// A real export stores them inline, so the default matches it; the shared
    /// variant exists to prove a reader copes with both.
    /// </param>
    /// <param name="sheetName">
    /// The worksheet name. A real export shapes it
    /// <c>YYYYMMDD_Export_Alle_Buchungen</c> and the export date is read back
    /// out of it, so a test that cares about that date sets this.
    /// </param>
    public static byte[] Build(
        IReadOnlyList<string> headerNames,
        IReadOnlyList<IReadOnlyDictionary<string, string>> dataRows,
        bool useSharedStrings = false,
        string sheetName = DefaultSheetName)
    {
        ArgumentNullException.ThrowIfNull(headerNames);
        ArgumentNullException.ThrowIfNull(dataRows);
        ArgumentNullException.ThrowIfNull(sheetName);

        var sharedStrings = new List<string>();
        var worksheetXml = BuildWorksheetXml(headerNames, dataRows, useSharedStrings, sharedStrings);

        using var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", ContentTypesXml);
            WriteEntry(archive, "_rels/.rels", PackageRelationshipsXml);
            WriteEntry(archive, "xl/workbook.xml", BuildWorkbookXml(sheetName));
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

                // A value the typed column cannot hold falls through to a text
                // cell rather than throwing. That is what a real export looks
                // like when it goes wrong, and a reader has to be testable
                // against it.
                if (headerNames[column] == FinanzguruColumns.BookingDate
                    && TryToDateTime(value, out var moment))
                {
                    AppendDateCell(builder, column, rowNumber, moment);
                }
                else if (headerNames[column] is FinanzguruColumns.Amount or FinanzguruColumns.Balance
                    && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var money))
                {
                    AppendMoneyCell(builder, column, rowNumber, money);
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

    private static void AppendDateCell(StringBuilder builder, int columnIndex, int rowNumber, DateTime value)
    {
        var reference = $"{ColumnLetters(columnIndex)}{rowNumber}";
        var serial = (value - ExcelEpoch).TotalDays;

        // A whole day is written without a fractional part, the way a real
        // export writes all but its PayPal rows.
        var text = serial == Math.Floor(serial)
            ? ((long)serial).ToString(CultureInfo.InvariantCulture)
            : serial.ToString("R", CultureInfo.InvariantCulture);

        builder.Append($"<c r=\"{reference}\" s=\"1\"><v>{text}</v></c>");
    }

    private static bool TryToDateTime(string value, out DateTime moment)
    {
        if (DateTime.TryParseExact(
                value,
                DateTimeValueFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out moment))
        {
            return true;
        }

        if (DateOnly.TryParseExact(value, "dd.MM.yyyy", out var date))
        {
            moment = date.ToDateTime(TimeOnly.MinValue);
            return true;
        }

        moment = default;
        return false;
    }

    private static void AppendMoneyCell(StringBuilder builder, int columnIndex, int rowNumber, decimal value)
    {
        var reference = $"{ColumnLetters(columnIndex)}{rowNumber}";

        builder.Append($"<c r=\"{reference}\" s=\"2\"><v>{value.ToString("0.00", CultureInfo.InvariantCulture)}</v></c>");
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

    private static string BuildWorkbookXml(string sheetName) =>
        $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + $"<workbook xmlns=\"{SpreadsheetNamespace}\" xmlns:r=\"{DocumentRelationshipsNamespace}\">"
        + $"<sheets><sheet name=\"{XmlEscape(sheetName)}\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
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
