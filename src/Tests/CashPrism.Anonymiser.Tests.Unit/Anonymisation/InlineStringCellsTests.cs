using CashPrism.Anonymiser.Anonymisation;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class InlineStringCellsTests
{
    private const string Worksheet =
        """<worksheet><sheetData>"""
        + """<row r="1">"""
        + """<c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c>"""
        + """<c r="B1" t="inlineStr"><is><t>Beguenstigter/Auftraggeber</t></is></c>"""
        + """</row>"""
        + """<row r="2">"""
        + """<c r="A2" s="1" t="n"><v>46200</v></c>"""
        + """<c r="B2" t="inlineStr"><is><t>Bakery</t></is></c>"""
        + """</row>"""
        + """<row r="3">"""
        + """<c r="A3" s="1" t="n"><v>46201</v></c>"""
        + """<c r="B3"></c>"""
        + """</row>"""
        + """</sheetData></worksheet>""";

    public sealed class ResolveColumnLetters
    {
        [Fact]
        public void Reads_The_Letter_Of_Every_Known_Header()
        {
            var letters = InlineStringCells.ResolveColumnLetters(Worksheet);

            Assert.Equal("A", letters[Infrastructure.Finanzguru.FinanzguruColumns.BookingDate]);
            Assert.Equal("B", letters[Infrastructure.Finanzguru.FinanzguruColumns.Counterparty]);
        }
    }

    public sealed class CollectValues
    {
        [Fact]
        public void Skips_The_Header_Row_And_Numeric_Cells()
        {
            var values = InlineStringCells.CollectValues(Worksheet, new HashSet<string>(["A", "B"]));

            Assert.Empty(values["A"]);
            Assert.Equal(["Bakery"], values["B"]);
        }

        [Fact]
        public void An_Empty_Cell_Contributes_No_Value()
        {
            var values = InlineStringCells.CollectValues(Worksheet, new HashSet<string>(["B"]));

            Assert.Single(values["B"]);
        }
    }

    public sealed class Rewrite
    {
        [Fact]
        public void Replaces_Only_Data_Row_Cells_In_The_Given_Columns()
        {
            var rewritten = InlineStringCells.Rewrite(
                Worksheet, new HashSet<string>(["B"]), (_, _) => "Counterparty 001");

            Assert.Contains("<t>Beguenstigter/Auftraggeber</t>", rewritten);
            Assert.Contains("""<c r="B2" t="inlineStr"><is><t>Counterparty 001</t></is></c>""", rewritten);
            Assert.DoesNotContain("Bakery", rewritten);
        }

        [Fact]
        public void Leaves_An_Empty_Cell_Untouched()
        {
            var rewritten = InlineStringCells.Rewrite(
                Worksheet, new HashSet<string>(["B"]), (_, _) => "Counterparty 001");

            Assert.Contains("""<c r="B3"></c>""", rewritten);
        }

        [Fact]
        public void Leaves_Cells_Outside_The_Given_Columns_Byte_Identical()
        {
            var rewritten = InlineStringCells.Rewrite(
                Worksheet, new HashSet<string>(["B"]), (_, _) => "Counterparty 001");

            Assert.Contains("""<c r="A2" s="1" t="n"><v>46200</v></c>""", rewritten);
        }
    }
}
