using System.Globalization;

namespace CashPrism.Application.Imports;

/// <summary>
/// One reason a file could not be imported: what went wrong, and the values that
/// fill the gaps in saying so — a column name, a row number, a worksheet name.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately not a sentence. The failure is found in layers that write
/// English (<c>core.language</c>), and the page that shows it writes German
/// (<c>AGENTS.md</c>), so what crosses between them is an identity the page
/// looks its own text up by.
/// </para>
/// <para>
/// Built through the factory methods, not the constructor. Each one fixes which
/// arguments a code carries and in which order, and that order is what the
/// placeholders of the matching translation count on — one place to keep in
/// step rather than every caller.
/// </para>
/// </remarks>
/// <param name="Code">What went wrong.</param>
/// <param name="Arguments">
/// The values that go with <paramref name="Code"/>, in the order its factory
/// method documents. Verbatim from the file, and invariant where they are
/// numbers.
/// </param>
public sealed record ImportError(ImportErrorCode Code, IReadOnlyList<string> Arguments)
{
    /// <summary>The file is not a workbook. Argument: the reason the library gave.</summary>
    /// <param name="detail">
    /// Why the spreadsheet library refused the file. Kept for the log, where it
    /// helps a bug report; it is the library's English and not meant for the page.
    /// </param>
    public static ImportError NotASpreadsheet(string detail)
        => new(ImportErrorCode.NotASpreadsheet, [detail]);

    /// <summary>The workbook carries no worksheet. No arguments.</summary>
    public static ImportError NoWorksheet()
        => new(ImportErrorCode.NoWorksheet, []);

    /// <summary>
    /// The worksheet name carries no export date. Arguments: the name, the suffix
    /// that has to follow the date.
    /// </summary>
    /// <param name="sheetName">The name the worksheet carries.</param>
    /// <param name="expectedSuffix">What has to follow the date in the name.</param>
    public static ImportError SheetNameWithoutExportDate(string sheetName, string expectedSuffix)
        => new(ImportErrorCode.SheetNameWithoutExportDate, [sheetName, expectedSuffix]);

    /// <summary>Known columns are missing. Argument: the column names, comma-separated.</summary>
    /// <param name="columns">The missing columns.</param>
    public static ImportError MissingColumns(IEnumerable<string> columns)
        => new(ImportErrorCode.MissingColumns, [List(columns)]);

    /// <summary>Known columns occur more than once. Argument: the column names, comma-separated.</summary>
    /// <param name="columns">The repeated columns.</param>
    public static ImportError DuplicateColumns(IEnumerable<string> columns)
        => new(ImportErrorCode.DuplicateColumns, [List(columns)]);

    /// <summary>A cell that has to carry a value is empty. Arguments: column, row.</summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    public static ImportError EmptyValue(string column, int row)
        => new(ImportErrorCode.EmptyValue, [column, Number(row)]);

    /// <summary>A date cell carries something else. Arguments: column, row, value.</summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="value">What the cell carries.</param>
    public static ImportError NotADate(string column, int row, string value)
        => new(ImportErrorCode.NotADate, [column, Number(row), value]);

    /// <summary>A money cell carries no number. Arguments: column, row, value.</summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="value">What the cell carries.</param>
    public static ImportError NotAnAmount(string column, int row, string value)
        => new(ImportErrorCode.NotAnAmount, [column, Number(row), value]);

    /// <summary>A money cell has more than two decimal places. Arguments: column, row, value.</summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="value">What the cell carries.</param>
    public static ImportError AmountNotWholeCents(string column, int row, string value)
        => new(ImportErrorCode.AmountNotWholeCents, [column, Number(row), value]);

    /// <summary>A money cell is too large to hold in cents. Arguments: column, row, value.</summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="value">What the cell carries.</param>
    public static ImportError AmountTooLarge(string column, int row, string value)
        => new(ImportErrorCode.AmountTooLarge, [column, Number(row), value]);

    /// <summary>A yes/no cell carries neither word. Arguments: column, row, value, yes, no.</summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="value">What the cell carries.</param>
    /// <param name="yes">The word the export writes for yes.</param>
    /// <param name="no">The word the export writes for no.</param>
    public static ImportError NotAFlag(string column, int row, string value, string yes, string no)
        => new(ImportErrorCode.NotAFlag, [column, Number(row), value, yes, no]);

    /// <summary>
    /// The split-type cell carries an unknown word. Arguments: column, row, value,
    /// the known words comma-separated.
    /// </summary>
    /// <param name="column">The header name of the column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="value">What the cell carries.</param>
    /// <param name="knownWords">The words the export is known to write.</param>
    public static ImportError UnknownSplitType(string column, int row, string value, IEnumerable<string> knownWords)
        => new(ImportErrorCode.UnknownSplitType, [column, Number(row), value, List(knownWords)]);

    /// <summary>
    /// A split part does not name its original. Arguments: split-type column, row,
    /// the column that should name the original.
    /// </summary>
    /// <param name="splitColumn">The header name of the split-type column.</param>
    /// <param name="row">The one-based worksheet row.</param>
    /// <param name="originalColumn">The header name of the column that is empty.</param>
    public static ImportError SplitPartWithoutOriginal(string splitColumn, int row, string originalColumn)
        => new(ImportErrorCode.SplitPartWithoutOriginal, [splitColumn, Number(row), originalColumn]);

    /// <summary>Two rows carry the same booking id. Arguments: row, the row that carried it first.</summary>
    /// <param name="row">The row that repeats the id.</param>
    /// <param name="firstRow">The row that carried it first.</param>
    public static ImportError RepeatedBookingId(int row, int firstRow)
        => new(ImportErrorCode.RepeatedBookingId, [Number(row), Number(firstRow)]);

    /// <summary>
    /// The code and its arguments, untranslated — what the log records, so a bug
    /// report does not depend on what the page happened to show.
    /// </summary>
    public override string ToString() => $"{Code}: {string.Join(" | ", Arguments)}";

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string List(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        return string.Join(", ", names);
    }
}
