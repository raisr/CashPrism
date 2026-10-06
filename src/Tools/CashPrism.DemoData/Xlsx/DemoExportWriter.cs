using CashPrism.DemoData.Generation;
using CashPrism.Infrastructure.Finanzguru;
using ClosedXML.Excel;

namespace CashPrism.DemoData.Xlsx;

/// <summary>
/// Writes the demo export as an <c>.xlsx</c>: one worksheet named after the
/// export date, the header in row 1 and every column a real export carries, in
/// its order.
/// </summary>
/// <remarks>
/// The file is an ordinary ClosedXML workbook. It does not imitate the Apache
/// POI internals a real export has — inline strings, a style per row — because
/// the reader reads both, and what the file is for is its content.
/// </remarks>
public static class DemoExportWriter
{
    /// <summary>The name of the file the writer produces.</summary>
    public const string FileName = "demo-export.xlsx";

    private const string DateFormat = "dd.MM.yyyy";

    /// <summary>The built-in number format <c>#,##0.00</c> the money columns carry.</summary>
    private const int MoneyFormatId = 4;

    /// <summary>
    /// Every column's value, by header name. A column is resolved by its name
    /// here as everywhere else, so the order of <see cref="FinanzguruColumns.All"/>
    /// alone decides where it lands.
    /// </summary>
    private static readonly Dictionary<string, Func<DemoRow, XLCellValue>> Columns = new(StringComparer.Ordinal)
    {
        [FinanzguruColumns.BookingDate] = row => row.Booking.Date,
        [FinanzguruColumns.AccountReference] = row => row.Booking.Account.Reference,
        [FinanzguruColumns.AccountName] = row => row.Booking.Account.Name,
        [FinanzguruColumns.Amount] = row => Money(row.Booking.AmountInCents),
        [FinanzguruColumns.Balance] = row => Money(row.BalanceInCents),
        [FinanzguruColumns.Currency] = _ => "EUR",
        [FinanzguruColumns.Counterparty] = row => row.Booking.Party.Name,
        [FinanzguruColumns.CounterpartyIban] = row => row.Booking.Party.Iban,
        [FinanzguruColumns.PaymentReference] = row => row.Booking.PaymentReference,
        [FinanzguruColumns.EndToEndReference] = _ => string.Empty,
        [FinanzguruColumns.MandateReference] = row => row.Booking.Party.MandateReference,
        [FinanzguruColumns.CreditorId] = row => row.Booking.Party.CreditorId,
        [FinanzguruColumns.MainCategory] = row => row.Booking.Category.Main,
        [FinanzguruColumns.SubCategory] = row => row.Booking.Category.Sub,
        [FinanzguruColumns.IsContract] = row => Flag(row.Booking.Contract is not null),
        [FinanzguruColumns.ContractInterval] = row => row.Booking.Contract?.Interval.Word ?? string.Empty,
        [FinanzguruColumns.ContractId] = row => row.Booking.Contract?.Id ?? string.Empty,
        [FinanzguruColumns.IsInternalTransfer] = row => Flag(row.Booking.IsInternalTransfer),
        [FinanzguruColumns.ExcludedFromDisposableIncome] = row => Flag(row.Booking.IsExcludedFromDisposableIncome),
        [FinanzguruColumns.TransactionKind] = row => row.Booking.TransactionKind,
        [FinanzguruColumns.AmountDirection] = row => row.AmountDirection,
        [FinanzguruColumns.Week] = row => row.Week,
        [FinanzguruColumns.Month] = row => row.Month,
        [FinanzguruColumns.Quarter] = row => row.Quarter,
        [FinanzguruColumns.Year] = row => row.Year,
        [FinanzguruColumns.BookingId] = row => row.Booking.BookingId,
        [FinanzguruColumns.OriginalReferenceId] = row => row.Booking.OriginalReferenceId,
        [FinanzguruColumns.SplitType] = row => row.Booking.SplitType,
        [FinanzguruColumns.Tags] = row => row.Booking.Tags,
    };

    /// <summary>
    /// Writes <paramref name="rows"/> as the export taken on
    /// <paramref name="until"/> into <paramref name="directory"/>, creating it
    /// where it is missing and replacing a file of the same name.
    /// </summary>
    /// <returns>The path of the written file.</returns>
    public static string Write(IReadOnlyList<DemoRow> rows, DateOnly until, string directory)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(DemoExport.SheetName(until));

        for (var column = 0; column < FinanzguruColumns.All.Count; column++)
        {
            sheet.Cell(1, column + 1).Value = FinanzguruColumns.All[column];
        }

        for (var index = 0; index < rows.Count; index++)
        {
            WriteRow(sheet, index + 2, rows[index]);
        }

        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, FileName);
        workbook.SaveAs(path);

        return path;
    }

    private static void WriteRow(IXLWorksheet sheet, int rowNumber, DemoRow row)
    {
        for (var column = 0; column < FinanzguruColumns.All.Count; column++)
        {
            var name = FinanzguruColumns.All[column];
            var value = Columns[name](row);

            // An empty text is a blank cell, the way the export leaves a
            // booking's unused columns.
            if (value.IsText && value.GetText().Length == 0)
            {
                continue;
            }

            var cell = sheet.Cell(rowNumber, column + 1);
            cell.Value = value;

            if (name == FinanzguruColumns.BookingDate)
            {
                cell.Style.NumberFormat.Format = DateFormat;
            }
            else if (name is FinanzguruColumns.Amount or FinanzguruColumns.Balance)
            {
                cell.Style.NumberFormat.NumberFormatId = MoneyFormatId;
            }
        }
    }

    private static double Money(long cents) => (double)(cents / 100m);

    private static string Flag(bool value) => value ? FinanzguruFlag.Yes : FinanzguruFlag.No;
}
