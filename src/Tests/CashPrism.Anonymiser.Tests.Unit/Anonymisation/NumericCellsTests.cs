using CashPrism.Anonymiser.Anonymisation;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class NumericCellsTests
{
    private const string Worksheet =
        """<worksheet><sheetData>"""
        + """<row r="1">"""
        + """<c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c>"""
        + """<c r="C1" t="inlineStr"><is><t>Betrag</t></is></c>"""
        + """</row>"""
        + """<row r="2">"""
        + """<c r="A2" s="1" t="n"><v>46200</v></c>"""
        + """<c r="C2" s="2"><v>-10.00</v></c>"""
        + """</row>"""
        + """</sheetData></worksheet>""";

    public sealed class Rewrite
    {
        [Fact]
        public void Replaces_Only_Data_Row_Cells_In_The_Given_Columns()
        {
            var rewritten = NumericCells.Rewrite(Worksheet, new HashSet<string>(["C"]), value => value * 2);

            Assert.Contains("""<c r="C2" s="2"><v>-20.00</v></c>""", rewritten);
        }

        [Fact]
        public void Leaves_Cells_Outside_The_Given_Columns_Byte_Identical()
        {
            var rewritten = NumericCells.Rewrite(Worksheet, new HashSet<string>(["C"]), value => value * 2);

            Assert.Contains("""<c r="A2" s="1" t="n"><v>46200</v></c>""", rewritten);
        }

        [Fact]
        public void Leaves_An_Inline_String_Header_Cell_Untouched_Even_When_Its_Letter_Matches()
        {
            var rewritten = NumericCells.Rewrite(Worksheet, new HashSet<string>(["C"]), value => value * 2);

            Assert.Contains("""<c r="C1" t="inlineStr"><is><t>Betrag</t></is></c>""", rewritten);
        }

        [Fact]
        public void Rounds_The_Transformed_Value_To_Two_Decimals_As_Formatted()
        {
            var rewritten = NumericCells.Rewrite(
                Worksheet, new HashSet<string>(["C"]), value => Math.Round(value * 0.333m, 2, MidpointRounding.AwayFromZero));

            Assert.Contains("""<c r="C2" s="2"><v>-3.33</v></c>""", rewritten);
        }
    }
}
