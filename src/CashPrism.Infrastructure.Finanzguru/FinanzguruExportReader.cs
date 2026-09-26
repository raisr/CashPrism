using System.Globalization;
using ClosedXML.Excel;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Reads a FinanzGuru "Alle Buchungen" export out of an <c>.xlsx</c> stream.
/// </summary>
/// <remarks>
/// <para>
/// The worksheet is taken **by position**, never by name: the name carries the
/// export date and therefore changes with every export. The header row is
/// resolved through <see cref="FinanzguruColumnMap"/>, so a column that moved
/// is followed and a column that vanished fails loudly.
/// </para>
/// <para>
/// Reading only. No deduplication, no persistence, and no mapping onto a
/// booking — what a row means is the import's decision, not this reader's.
/// </para>
/// <para>
/// The stream has to be seekable, which is what ClosedXML needs to open a
/// workbook. A caller holding a forward-only stream — an upload, for instance —
/// copies it into memory first; the measured export is 1.15 MB.
/// </para>
/// </remarks>
public sealed class FinanzguruExportReader
{
    /// <summary>The worksheet row the header sits in.</summary>
    private const int HeaderRowNumber = 1;

    /// <summary>
    /// Round-trip shape for the date column in <see cref="FinanzguruExportRow.Values"/>.
    /// Seconds precision, because a fractional date serial converts to a time
    /// with sub-second noise that the export never meant to carry.
    /// </summary>
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>
    /// Reads <paramref name="stream"/> as a FinanzGuru export.
    /// </summary>
    /// <param name="stream">The <c>.xlsx</c> to read. Has to be readable and seekable.</param>
    /// <returns>
    /// A successful result carrying the export, or a failure listing everything
    /// that is wrong with the file. See <see cref="FinanzguruExportReadResult"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public FinanzguruExportReadResult Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        XLWorkbook workbook;

        try
        {
            workbook = new XLWorkbook(stream);
        }
        // Deliberately broad, and deliberately wrapped around this one
        // third-party call and nothing else. A file that is not a workbook
        // surfaces as an IOException, an InvalidDataException, a
        // FileFormatException, an XmlException or a ClosedXMLException
        // depending on how far the parse got, and that list is open-ended —
        // naming it would leave the next variant to escape as a stack trace in
        // front of a person who picked the wrong file. Every one of them means
        // the same thing here, and no code of ours runs inside the try.
        catch (Exception exception)
        {
            return FinanzguruExportReadResult.Failure(
                $"The file could not be opened as a spreadsheet: {exception.Message}");
        }

        using (workbook)
        {
            return Read(workbook);
        }
    }

    private static FinanzguruExportReadResult Read(XLWorkbook workbook)
    {
        if (workbook.Worksheets.Count == 0)
        {
            return FinanzguruExportReadResult.Failure("The workbook carries no worksheet.");
        }

        var worksheet = workbook.Worksheet(1);

        if (!FinanzguruSheetName.TryParseExportDate(worksheet.Name, out var exportedOn))
        {
            return FinanzguruExportReadResult.Failure(
                $"The worksheet is named '{worksheet.Name}'; expected a name shaped "
                + $"'YYYYMMDD{FinanzguruSheetName.Suffix}', which is where the export date is read from.");
        }

        var columnMap = ResolveColumns(worksheet, out var headerFailure);

        if (columnMap is null)
        {
            return headerFailure!;
        }

        var rows = new List<FinanzguruExportRow>();
        var errors = new List<string>();

        foreach (var row in worksheet.RowsUsed().Where(r => r.RowNumber() > HeaderRowNumber))
        {
            var read = ReadRow(row, columnMap.Columns, errors);

            if (read is not null)
            {
                rows.Add(read);
            }
        }

        if (errors.Count > 0)
        {
            return FinanzguruExportReadResult.Failure([.. errors]);
        }

        return FinanzguruExportReadResult.Success(
            new FinanzguruExport(exportedOn, rows, columnMap.UnknownColumns));
    }

    private static FinanzguruColumnMapResult? ResolveColumns(
        IXLWorksheet worksheet,
        out FinanzguruExportReadResult? failure)
    {
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var headerRow = worksheet.Row(HeaderRowNumber);

        var headerNames = Enumerable
            .Range(1, lastColumn)
            .Select(column => headerRow.Cell(column).GetString())
            .ToArray();

        var result = FinanzguruColumnMap.Resolve(headerNames);

        if (result.IsSuccess)
        {
            failure = null;
            return result;
        }

        var errors = new List<string>();

        if (result.MissingColumns.Count > 0)
        {
            errors.Add($"The export is missing the column(s) {Quote(result.MissingColumns)}.");
        }

        if (result.DuplicateColumns.Count > 0)
        {
            errors.Add(
                $"The export carries the column(s) {Quote(result.DuplicateColumns)} more than once; "
                + "which one to read is undecidable.");
        }

        failure = FinanzguruExportReadResult.Failure([.. errors]);
        return null;
    }

    private static FinanzguruExportRow? ReadRow(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        List<string> errors)
    {
        var rowNumber = row.RowNumber();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (column, index) in columns)
        {
            values[column] = AsText(row.Cell(index + 1).Value);
        }

        var errorsBefore = errors.Count;

        var bookedOn = ReadDate(row, columns, FinanzguruColumns.BookingDate, errors);
        var amountInCents = ReadAmount(row, columns, FinanzguruColumns.Amount, errors);
        var balanceInCents = ReadAmount(row, columns, FinanzguruColumns.Balance, errors);

        if (errors.Count > errorsBefore)
        {
            return null;
        }

        return new FinanzguruExportRow(
            rowNumber,
            bookedOn!.Value,
            amountInCents!.Value,
            balanceInCents!.Value,
            values);
    }

    private static DateTime? ReadDate(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        string column,
        List<string> errors)
    {
        var value = row.Cell(columns[column] + 1).Value;

        // Strictly a date cell. A date is stored as a serial number and only
        // the number format makes it read as one, so a bare number would be
        // convertible — but every date cell in both measured exports carries
        // the format, and tolerating one that does not would be guessing at a
        // file we have never seen.
        if (value.IsDateTime)
        {
            return value.GetDateTime();
        }

        errors.Add(
            $"Column '{column}' in row {row.RowNumber()} {Describe(value)}; expected a date.");

        return null;
    }

    private static long? ReadAmount(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        string column,
        List<string> errors)
    {
        var rowNumber = row.RowNumber();
        var value = row.Cell(columns[column] + 1).Value;

        if (!value.IsNumber)
        {
            errors.Add($"Column '{column}' in row {rowNumber} {Describe(value)}; expected an amount.");
            return null;
        }

        var result = FinanzguruAmount.ToCents(value.GetNumber(), column, rowNumber);

        if (!result.IsSuccess)
        {
            errors.Add(result.Error!);
            return null;
        }

        return result.AmountInCents;
    }

    /// <summary>
    /// Renders a cell for <see cref="FinanzguruExportRow.Values"/>: invariant,
    /// lossless and independent of the reader's culture, so the same export
    /// produces the same text on every machine.
    /// </summary>
    private static string AsText(XLCellValue value) => value.Type switch
    {
        XLDataType.Blank => string.Empty,
        XLDataType.Text => value.GetText(),
        XLDataType.DateTime => value.GetDateTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture),

        // Through decimal, so a whole number does not pick up a ".0" and a money
        // cell does not pick up the binary float's trailing digits.
        XLDataType.Number => ((decimal)value.GetNumber()).ToString(CultureInfo.InvariantCulture),

        _ => value.ToString(CultureInfo.InvariantCulture),
    };

    private static string Describe(XLCellValue value)
        => value.IsBlank ? "is empty" : $"carries '{value.ToString(CultureInfo.InvariantCulture)}'";

    private static string Quote(IReadOnlyList<string> names)
        => string.Join(", ", names.Select(name => $"'{name}'"));
}
