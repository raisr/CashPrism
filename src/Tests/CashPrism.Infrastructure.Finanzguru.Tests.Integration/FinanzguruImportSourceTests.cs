using System.Text;
using CashPrism.Application.Imports;
using CashPrism.TestSupport.Xlsx;

namespace CashPrism.Infrastructure.Finanzguru.Tests.Integration;

public sealed class FinanzguruImportSourceTests
{
    private static readonly Guid ARunId = Guid.Parse("8f3b1c2d-4e5f-4a6b-8c9d-0e1f2a3b4c5d");

    private static Task<Application.Imports.ImportSourceResult> ReadWorkbookAsync(
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        string sheetName = XlsxTestWorkbook.DefaultSheetName)
        => ReadBytesAsync(XlsxTestWorkbook.Build(FinanzguruColumns.All, rows, sheetName: sheetName));

    private static async Task<Application.Imports.ImportSourceResult> ReadBytesAsync(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);

        return await new FinanzguruImportSource().ReadAsync(stream, ARunId);
    }

    private static Dictionary<string, string> Row(
        string bookingId = FinanzguruTestRow.AFingerprint,
        string isInternalTransfer = FinanzguruFlag.No)
        => FinanzguruTestRow.Create(bookingId, isInternalTransfer: isInternalTransfer);

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Turns_Every_Row_Into_A_Booking()
        {
            var result = await ReadWorkbookAsync(
                [Row(bookingId: new string('a', 40)), Row(bookingId: new string('b', 40))]);

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors));
            Assert.Equal(2, result.Data!.Bookings.Count);
        }

        [Fact]
        public async Task Hands_On_The_Worksheet_Name_Verbatim()
        {
            var result = await ReadWorkbookAsync([Row()], "20260415_Export_Alle_Buchungen");

            Assert.Equal("20260415_Export_Alle_Buchungen", result.Data!.SheetName);
            Assert.Equal(new DateOnly(2026, 4, 15), result.Data.ExportedOn);
        }

        [Fact]
        public async Task Stamps_Every_Booking_With_The_Run_It_Was_Read_For()
        {
            var result = await ReadWorkbookAsync([Row()]);

            Assert.Equal(ARunId, result.Data!.Bookings.Single().Booking.SourceImportRunId);
        }

        [Fact]
        public async Task Keeps_The_Whole_Row_As_Json_Beside_The_Booking()
        {
            var result = await ReadWorkbookAsync([Row()]);

            var json = result.Data!.Bookings.Single().RawJson;

            // Every known column is in there, including the ones the projection
            // drops — that is what the raw row is for.
            Assert.Contains("\"Analyse-Woche\":", json, StringComparison.Ordinal);
            Assert.Contains("\"Tags\":", json, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Numbers_A_Booking_By_Its_Worksheet_Row()
        {
            var result = await ReadWorkbookAsync(
                [Row(bookingId: new string('a', 40)), Row(bookingId: new string('b', 40))]);

            Assert.Equal([2, 3], result.Data!.Bookings.Select(booking => booking.RowNumber));
        }

        [Fact]
        public async Task Passes_On_The_Reason_The_File_Could_Not_Be_Read()
        {
            var result = await ReadBytesAsync(Encoding.UTF8.GetBytes("This is not an xlsx."));

            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task Fails_And_Names_The_Row_A_Booking_Could_Not_Be_Built_From()
        {
            var result = await ReadWorkbookAsync(
                [Row(), Row(bookingId: new string('b', 40), isInternalTransfer: "vielleicht")]);

            Assert.False(result.IsSuccess);
            Assert.Equivalent(
                ImportError.NotAFlag(FinanzguruColumns.IsInternalTransfer, 3, "vielleicht", FinanzguruFlag.Yes, FinanzguruFlag.No),
                result.Errors.Single(),
                strict: true);
        }

        [Fact]
        public async Task Reports_A_Column_The_Export_Gained()
        {
            var headerNames = FinanzguruColumns.All.Append("Analyse-Nebelkerze").ToArray();

            var result = await ReadBytesAsync(XlsxTestWorkbook.Build(headerNames, [Row()]));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors));
            Assert.Equal(["Analyse-Nebelkerze"], result.Data!.UnknownColumns);
        }

        [Fact]
        public async Task Rejects_A_Missing_Stream()
            => await Assert.ThrowsAsync<ArgumentNullException>(
                () => new FinanzguruImportSource().ReadAsync(null!, ARunId));
    }
}
