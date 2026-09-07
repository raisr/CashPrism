using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class WorksheetAnonymiserTests
{
    public sealed class Rewrite
    {
        [Fact]
        public void Replaces_A_Cell_With_Its_Dictionary_Entry_And_Leaves_Kept_Cells_Alone()
        {
            const string worksheetXml =
                """<worksheet><sheetData>"""
                + """<row r="1">"""
                + """<c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c>"""
                + """<c r="B1" t="inlineStr"><is><t>Beguenstigter/Auftraggeber</t></is></c>"""
                + """</row>"""
                + """<row r="2">"""
                + """<c r="A2" s="1" t="n"><v>46200</v></c>"""
                + """<c r="B2" t="inlineStr"><is><t>Bakery</t></is></c>"""
                + """</row>"""
                + """</sheetData></worksheet>""";

            var columnLetters = new Dictionary<string, string>
            {
                [FinanzguruColumns.BookingDate] = "A",
                [FinanzguruColumns.Counterparty] = "B",
            };

            var values = AnonymisationDictionaries.ReplacedColumns.ToDictionary(
                column => column, _ => (IReadOnlyList<string>)Array.Empty<string>(), StringComparer.Ordinal);
            values[FinanzguruColumns.Counterparty] = ["Bakery"];

            var dictionaries = AnonymisationDictionaries.Build([values]);

            var rewritten = WorksheetAnonymiser.Rewrite(worksheetXml, columnLetters, dictionaries);

            Assert.Contains("""<c r="A2" s="1" t="n"><v>46200</v></c>""", rewritten);
            Assert.Contains(
                $"""<c r="B2" t="inlineStr"><is><t>{dictionaries.Replace(FinanzguruColumns.Counterparty, "Bakery")}</t></is></c>""",
                rewritten);
            Assert.DoesNotContain("Bakery", rewritten);
        }
    }
}
