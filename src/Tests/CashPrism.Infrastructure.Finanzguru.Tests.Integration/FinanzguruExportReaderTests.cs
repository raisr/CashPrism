using System.Text;
using CashPrism.TestSupport.Xlsx;

namespace CashPrism.Infrastructure.Finanzguru.Tests.Integration;

public sealed class FinanzguruExportReaderTests
{
    public sealed class Read
    {
        [Fact]
        public void Reads_The_Export_Date_From_The_Worksheet_Name()
        {
            var result = ReadWorkbook([Row()], sheetName: "20260415_Export_Alle_Buchungen");

            Assert.True(result.IsSuccess, Join(result.Errors));
            Assert.Equal(new DateOnly(2026, 4, 15), result.Export!.ExportedOn);
        }

        [Fact]
        public void Reads_Every_Data_Row()
        {
            var result = ReadWorkbook([Row(), Row(), Row()]);

            Assert.True(result.IsSuccess, Join(result.Errors));
            Assert.Equal(3, result.Export!.Rows.Count);
        }

        [Fact]
        public void Numbers_A_Row_By_Its_Worksheet_Row()
        {
            var result = ReadWorkbook([Row(), Row()]);

            Assert.Equal([2, 3], result.Export!.Rows.Select(row => row.RowNumber));
        }

        [Fact]
        public void Reads_A_Whole_Day_Booking_Date_Without_A_Time()
        {
            var result = ReadWorkbook([Row(bookingDate: "03.03.2026")]);

            Assert.Equal(new DateTime(2026, 3, 3, 0, 0, 0), result.Export!.Rows[0].BookedOn);
        }

        /// <summary>
        /// 52 of 6,324 measured rows carry a time in the date serial — the
        /// PayPal bookings. The cell format hides it, so a reader that drops it
        /// does so silently. This one keeps it.
        /// </summary>
        [Fact]
        public void Keeps_The_Time_Of_A_Fractional_Date_Serial()
        {
            var result = ReadWorkbook([Row(bookingDate: "03.03.2026 14:35:12")]);

            Assert.Equal(new DateTime(2026, 3, 3, 14, 35, 12), result.Export!.Rows[0].BookedOn);
        }

        [Fact]
        public void Reads_The_Amount_As_Whole_Cents()
        {
            var result = ReadWorkbook([Row(amount: "-1234.56")]);

            Assert.Equal(-123456L, result.Export!.Rows[0].AmountInCents);
        }

        [Fact]
        public void Reads_The_Balance_As_Whole_Cents()
        {
            var result = ReadWorkbook([Row(balance: "98.70")]);

            Assert.Equal(9870L, result.Export!.Rows[0].BalanceInCents);
        }

        [Fact]
        public void Carries_Every_Known_Column_In_The_Values()
        {
            var result = ReadWorkbook([Row()]);

            Assert.Equal(
                FinanzguruColumns.All.Order(),
                result.Export!.Rows[0].Values.Keys.Order());
        }

        [Fact]
        public void Renders_An_Empty_Cell_As_An_Empty_Value()
        {
            var result = ReadWorkbook([Row()]);

            Assert.Equal(string.Empty, result.Export!.Rows[0].Values[FinanzguruColumns.EndToEndReference]);
        }

        [Fact]
        public void Renders_A_Money_Cell_Without_The_Binary_Floats_Trailing_Digits()
        {
            var result = ReadWorkbook([Row(amount: "1234.56")]);

            Assert.Equal("1234.56", result.Export!.Rows[0].Values[FinanzguruColumns.Amount]);
        }

        [Fact]
        public void Renders_A_Date_Cell_As_A_Round_Trip_Timestamp()
        {
            var result = ReadWorkbook([Row(bookingDate: "03.03.2026 14:35:12")]);

            Assert.Equal("2026-03-03T14:35:12", result.Export!.Rows[0].Values[FinanzguruColumns.BookingDate]);
        }

        [Fact]
        public void Renders_A_Text_Cell_Verbatim()
        {
            var result = ReadWorkbook([Row(counterparty: "  Bakery  ")]);

            Assert.Equal("  Bakery  ", result.Export!.Rows[0].Values[FinanzguruColumns.Counterparty]);
        }

        [Fact]
        public void Reads_A_Workbook_That_Stores_Its_Strings_In_The_Shared_Table()
        {
            var bytes = XlsxTestWorkbook.Build(FinanzguruColumns.All, [Row()], useSharedStrings: true);

            var result = ReadBytes(bytes);

            Assert.True(result.IsSuccess, Join(result.Errors));
            Assert.Equal("Bakery", result.Export!.Rows[0].Values[FinanzguruColumns.Counterparty]);
        }

        [Fact]
        public void Reads_An_Export_That_Has_No_Data_Rows()
        {
            var result = ReadWorkbook([]);

            Assert.True(result.IsSuccess, Join(result.Errors));
            Assert.Empty(result.Export!.Rows);
        }

        [Fact]
        public void Fails_And_Names_A_Missing_Column()
        {
            var headerNames = FinanzguruColumns.All.Where(c => c != FinanzguruColumns.Tags).ToArray();

            var result = ReadBytes(XlsxTestWorkbook.Build(headerNames, [Row()]));

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.Tags, Join(result.Errors), StringComparison.Ordinal);
        }

        [Fact]
        public void Fails_And_Names_A_Duplicated_Column()
        {
            var headerNames = FinanzguruColumns.All.Append(FinanzguruColumns.Tags).ToArray();

            var result = ReadBytes(XlsxTestWorkbook.Build(headerNames, [Row()]));

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.Tags, Join(result.Errors), StringComparison.Ordinal);
        }

        /// <summary>
        /// An export that gained a column is still readable, and what the column
        /// means is the caller's decision — see <c>docs/finanzguru-export.md</c>.
        /// </summary>
        [Fact]
        public void Reports_An_Unknown_Column_And_Keeps_Reading()
        {
            var headerNames = FinanzguruColumns.All.Append("Analyse-Nebelkerze").ToArray();

            var result = ReadBytes(XlsxTestWorkbook.Build(headerNames, [Row()]));

            Assert.True(result.IsSuccess, Join(result.Errors));
            Assert.Equal(["Analyse-Nebelkerze"], result.Export!.UnknownColumns);
            Assert.Single(result.Export.Rows);
        }

        [Fact]
        public void Fails_And_Names_The_Worksheet_When_Its_Name_Carries_No_Export_Date()
        {
            var result = ReadWorkbook([Row()], sheetName: "Tabelle1");

            Assert.False(result.IsSuccess);
            Assert.Contains("Tabelle1", Join(result.Errors), StringComparison.Ordinal);
        }

        [Fact]
        public void Fails_And_Names_The_Column_When_An_Amount_Is_Not_A_Number()
        {
            var result = ReadWorkbook([Row(amount: "not an amount")]);

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.Amount, Join(result.Errors), StringComparison.Ordinal);
            Assert.Contains("row 2", Join(result.Errors), StringComparison.Ordinal);
        }

        [Fact]
        public void Fails_And_Names_The_Column_When_The_Booking_Date_Is_Not_A_Date()
        {
            var result = ReadWorkbook([Row(bookingDate: "not a date")]);

            Assert.False(result.IsSuccess);
            Assert.Contains(FinanzguruColumns.BookingDate, Join(result.Errors), StringComparison.Ordinal);
        }

        [Fact]
        public void Fails_When_The_File_Is_Not_A_Spreadsheet()
        {
            var result = ReadBytes(Encoding.UTF8.GetBytes("This is not an xlsx."));

            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Rejects_A_Missing_Stream()
            => Assert.Throws<ArgumentNullException>(() => new FinanzguruExportReader().Read(null!));

        private static FinanzguruExportReadResult ReadWorkbook(
            IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
            string sheetName = XlsxTestWorkbook.DefaultSheetName)
            => ReadBytes(XlsxTestWorkbook.Build(FinanzguruColumns.All, rows, sheetName: sheetName));

        private static FinanzguruExportReadResult ReadBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);

            return new FinanzguruExportReader().Read(stream);
        }

        /// <summary>
        /// A data row carrying a value in every column the reader types, so a
        /// test only states the value it is actually about.
        /// </summary>
        private static Dictionary<string, string> Row(
            string bookingDate = "03.03.2026",
            string amount = "-12.34",
            string balance = "100.00",
            string counterparty = "Bakery")
            => new(StringComparer.Ordinal)
            {
                [FinanzguruColumns.BookingDate] = bookingDate,
                [FinanzguruColumns.Amount] = amount,
                [FinanzguruColumns.Balance] = balance,
                [FinanzguruColumns.Counterparty] = counterparty,
                [FinanzguruColumns.Currency] = "EUR",
                [FinanzguruColumns.AccountReference] = "DE00000000000000000000",
                [FinanzguruColumns.BookingId] = new string('a', 40),
            };

        private static string Join(IReadOnlyList<string> errors) => string.Join(" | ", errors);
    }
}
