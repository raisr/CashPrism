using CashPrism.Anonymiser.Xlsx;

namespace CashPrism.Anonymiser.Tests.Unit.Xlsx;

public sealed class WorksheetRowLimiterTests
{
    private const string Worksheet =
        """<worksheet><sheetData>"""
        + """<row r="1"><c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c></row>"""
        + """<row r="2"><c r="A2" t="inlineStr"><is><t>1</t></is></c></row>"""
        + """<row r="3"><c r="A3" t="inlineStr"><is><t>2</t></is></c></row>"""
        + """<row r="4"><c r="A4" t="inlineStr"><is><t>3</t></is></c></row>"""
        + """</sheetData></worksheet>""";

    public sealed class Limit
    {
        [Fact]
        public void Null_MaxRows_Returns_The_Worksheet_Unchanged()
        {
            var (worksheetXml, dataRowCount) = WorksheetRowLimiter.Limit(Worksheet, maxRows: null, dataRowCount: 3);

            Assert.Same(Worksheet, worksheetXml);
            Assert.Equal(3, dataRowCount);
        }

        [Fact]
        public void MaxRows_Larger_Than_The_Row_Count_Returns_The_Worksheet_Unchanged()
        {
            var (worksheetXml, dataRowCount) = WorksheetRowLimiter.Limit(Worksheet, maxRows: 10, dataRowCount: 3);

            Assert.Same(Worksheet, worksheetXml);
            Assert.Equal(3, dataRowCount);
        }

        [Fact]
        public void MaxRows_Keeps_The_Header_Plus_The_First_N_Data_Rows()
        {
            var (worksheetXml, dataRowCount) = WorksheetRowLimiter.Limit(Worksheet, maxRows: 2, dataRowCount: 3);

            Assert.Equal(2, dataRowCount);
            Assert.Contains("""<row r="1">""", worksheetXml);
            Assert.Contains("""<row r="2">""", worksheetXml);
            Assert.Contains("""<row r="3">""", worksheetXml);
            Assert.DoesNotContain("""<row r="4">""", worksheetXml);
            Assert.EndsWith("</sheetData></worksheet>", worksheetXml);
        }

        [Fact]
        public void MaxRows_Of_Zero_Keeps_Only_The_Header()
        {
            var (worksheetXml, dataRowCount) = WorksheetRowLimiter.Limit(Worksheet, maxRows: 0, dataRowCount: 3);

            Assert.Equal(0, dataRowCount);
            Assert.Contains("""<row r="1">""", worksheetXml);
            Assert.DoesNotContain("""<row r="2">""", worksheetXml);
        }
    }
}
