using System.Globalization;
using System.Text.RegularExpressions;
using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class SyntheticValuesTests
{
    public sealed class Rewrite
    {
        private static readonly IReadOnlyDictionary<string, string> ColumnLetters = new Dictionary<string, string>
        {
            [FinanzguruColumns.BookingDate] = "A",
            [FinanzguruColumns.Amount] = "D",
            [FinanzguruColumns.Balance] = "E",
            [FinanzguruColumns.Week] = "V",
            [FinanzguruColumns.Month] = "W",
            [FinanzguruColumns.Quarter] = "X",
            [FinanzguruColumns.Year] = "Y",
            [FinanzguruColumns.BookingId] = "Z",
            [FinanzguruColumns.OriginalReferenceId] = "AA",
            [FinanzguruColumns.SplitType] = "AB",
        };

        /// <summary>
        /// Shaped like a real export: numeric cells with a style and <c>t="n"</c>,
        /// numbers printed the way Java prints a double, a row with a time of day,
        /// a zero amount, and one split booking with both of its parts.
        /// </summary>
        private static readonly string WorksheetXml =
            """<worksheet><sheetData>"""
            + """<row r="1"><c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c></row>"""
            + Row(2, "46270.0", "-12.5", "1500.25", "2026-39", "2026-09", "2026-Q3", "2026.0", "id-a", "", "")
            + Row(3, "46269.41666666667", "250.0", "1750.75", "2026-39", "2026-09", "2026-Q3", "2026.0", "id-b", "", "")
            + Row(4, "46268.0", "0.0", "0.0", "2026-38", "2026-09", "2026-Q3", "2026.0", "id-c", "", "")
            + Row(5, "46267.0", "-100.0", "900.0", "2026-38", "2026-09", "2026-Q3", "2026.0", "id-orig", "", "Original")
            + Row(6, "46267.0", "-60.0", "900.0", "2026-38", "2026-09", "2026-Q3", "2026.0", "id-p1", "id-orig", "Teilbuchung")
            + Row(7, "46267.0", "-40.0", "900.0", "2026-38", "2026-09", "2026-Q3", "2026.0", "id-p2", "id-orig", "Restbetrag")
            + """</sheetData></worksheet>""";

        private static readonly Regex NumberAsJavaPrintsIt = new(@"^-?[0-9]+\.[0-9]{1,2}$");

        [Fact]
        public void Gives_The_First_Data_Row_The_First_Date()
            => Assert.Equal("36892.0", Value(Rewritten(), "A2"));

        [Fact]
        public void Keeps_The_Time_Of_Day_Of_A_Row_That_Has_One()
            => Assert.Equal("36891.41666666667", Value(Rewritten(), "A3"));

        [Fact]
        public void Keeps_The_Rows_In_Descending_Date_Order()
        {
            var rewritten = Rewritten();

            var serials = Enumerable.Range(2, 6).Select(row => Number(rewritten, $"A{row}")).ToArray();

            Assert.Equal(serials.OrderByDescending(serial => serial), serials);
        }

        [Theory]
        [InlineData("D2")]
        [InlineData("D3")]
        [InlineData("E2")]
        [InlineData("E3")]
        [InlineData("E5")]
        public void Replaces_Every_Amount_That_Is_Not_Zero(string cell)
            => Assert.NotEqual(Number(WorksheetXml, cell), Number(Rewritten(), cell));

        [Theory]
        [InlineData("D2")]
        [InlineData("D3")]
        [InlineData("D6")]
        public void Keeps_The_Sign_Of_Betrag(string cell)
            => Assert.Equal(Math.Sign(Number(WorksheetXml, cell)), Math.Sign(Number(Rewritten(), cell)));

        [Theory]
        [InlineData("D4")]
        [InlineData("E4")]
        public void Leaves_A_Zero_At_Zero(string cell)
            => Assert.Equal(0m, Number(Rewritten(), cell));

        [Fact]
        public void Makes_The_Parts_Of_A_Split_Add_Up_To_Their_Original()
        {
            var rewritten = Rewritten();

            Assert.Equal(Number(rewritten, "D5"), Number(rewritten, "D6") + Number(rewritten, "D7"));
        }

        [Theory]
        [InlineData("D2")]
        [InlineData("E3")]
        [InlineData("Y2")]
        public void Writes_A_Number_The_Way_The_Export_Does(string cell)
            => Assert.Matches(NumberAsJavaPrintsIt, Value(Rewritten(), cell));

        [Fact]
        public void Recomputes_The_Period_Columns_From_The_Generated_Date()
        {
            var rewritten = Rewritten();

            Assert.Equal(
                ["2001-01", "2001-01", "2001-Q1", "2001.0"],
                [Value(rewritten, "V2"), Value(rewritten, "W2"), Value(rewritten, "X2"), Value(rewritten, "Y2")]);
        }

        [Fact]
        public void Keeps_The_Style_And_Type_Of_A_Replaced_Cell()
            => Assert.Matches("""<c r="D2" s="7" t="n"><v>[^<]+</v></c>""", Rewritten());

        [Fact]
        public void Leaves_The_Header_Row_And_The_Ids_Alone()
        {
            var rewritten = Rewritten();

            Assert.Contains("""<row r="1"><c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c></row>""", rewritten);
            Assert.Contains("""<c r="Z5" t="inlineStr"><is><t>id-orig</t></is></c>""", rewritten);
        }

        [Fact]
        public void Produces_The_Same_Output_Every_Time()
            => Assert.Equal(Rewritten(), Rewritten());

        private static string Rewritten() => SyntheticValues.Rewrite(WorksheetXml, ColumnLetters);

        private static string Row(
            int row,
            string date,
            string amount,
            string balance,
            string week,
            string month,
            string quarter,
            string year,
            string bookingId,
            string originalReferenceId,
            string splitType)
            => $"""<row r="{row}" spans="1:28">"""
                + $"""<c r="A{row}" s="5" t="n"><v>{date}</v></c>"""
                + $"""<c r="D{row}" s="7" t="n"><v>{amount}</v></c>"""
                + $"""<c r="E{row}" s="7" t="n"><v>{balance}</v></c>"""
                + $"""<c r="V{row}" t="inlineStr"><is><t>{week}</t></is></c>"""
                + $"""<c r="W{row}" t="inlineStr"><is><t>{month}</t></is></c>"""
                + $"""<c r="X{row}" t="inlineStr"><is><t>{quarter}</t></is></c>"""
                + $"""<c r="Y{row}" t="n"><v>{year}</v></c>"""
                + $"""<c r="Z{row}" t="inlineStr"><is><t>{bookingId}</t></is></c>"""
                + (originalReferenceId.Length == 0
                    ? $"""<c r="AA{row}"></c>"""
                    : $"""<c r="AA{row}" t="inlineStr"><is><t>{originalReferenceId}</t></is></c>""")
                + (splitType.Length == 0
                    ? $"""<c r="AB{row}"></c>"""
                    : $"""<c r="AB{row}" t="inlineStr"><is><t>{splitType}</t></is></c>""")
                + "</row>";

        private static string Value(string worksheetXml, string cell)
            => Regex.Match(worksheetXml, $"""<c r="{cell}"[^>]*>(?:<v>(?<v>[^<]*)</v>|<is><t>(?<v>[^<]*)</t></is>)</c>""")
                .Groups["v"].Value;

        private static decimal Number(string worksheetXml, string cell)
            => decimal.Parse(Value(worksheetXml, cell), NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
