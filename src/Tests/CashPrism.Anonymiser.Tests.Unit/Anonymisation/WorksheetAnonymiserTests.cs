using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class WorksheetAnonymiserTests
{
    public sealed class Rewrite
    {
        private static readonly IReadOnlyDictionary<string, string> ColumnLetters = new Dictionary<string, string>
        {
            [FinanzguruColumns.BookingDate] = "A",
            [FinanzguruColumns.Counterparty] = "B",
            [FinanzguruColumns.Amount] = "C",
            [FinanzguruColumns.Balance] = "D",
        };

        private const string WorksheetXml =
            """<worksheet><sheetData>"""
            + """<row r="1">"""
            + """<c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c>"""
            + """<c r="B1" t="inlineStr"><is><t>Beguenstigter/Auftraggeber</t></is></c>"""
            + """<c r="C1" t="inlineStr"><is><t>Betrag</t></is></c>"""
            + """<c r="D1" t="inlineStr"><is><t>Kontostand</t></is></c>"""
            + """</row>"""
            + """<row r="2">"""
            + """<c r="A2" s="1" t="n"><v>46200</v></c>"""
            + """<c r="B2" t="inlineStr"><is><t>Bakery</t></is></c>"""
            + """<c r="C2" s="2"><v>-10.00</v></c>"""
            + """<c r="D2" s="2"><v>200.00</v></c>"""
            + """</row>"""
            + """</sheetData></worksheet>""";

        [Fact]
        public void Replaces_A_Cell_With_Its_Dictionary_Entry_And_Leaves_Kept_Cells_Alone()
        {
            var dictionaries = DictionariesSeenInWorksheetXml();

            var rewritten = WorksheetAnonymiser.Rewrite(WorksheetXml, ColumnLetters, dictionaries, scale: 1.0m);

            Assert.Contains("""<c r="A2" s="1" t="n"><v>46200</v></c>""", rewritten);
            Assert.Contains(
                $"""<c r="B2" t="inlineStr"><is><t>{dictionaries.Replace(FinanzguruColumns.Counterparty, "Bakery")}</t></is></c>""",
                rewritten);
            Assert.DoesNotContain("Bakery", rewritten);
        }

        [Fact]
        public void A_Scale_Of_One_Leaves_The_Money_Columns_Byte_Identical()
        {
            var dictionaries = DictionariesSeenInWorksheetXml();

            var rewritten = WorksheetAnonymiser.Rewrite(WorksheetXml, ColumnLetters, dictionaries, scale: 1.0m);

            Assert.Contains("""<c r="C2" s="2"><v>-10.00</v></c>""", rewritten);
            Assert.Contains("""<c r="D2" s="2"><v>200.00</v></c>""", rewritten);
        }

        [Fact]
        public void Scales_Betrag_And_Kontostand_Together_Rounded_To_Two_Decimals()
        {
            var dictionaries = DictionariesSeenInWorksheetXml();

            var rewritten = WorksheetAnonymiser.Rewrite(WorksheetXml, ColumnLetters, dictionaries, scale: 0.5m);

            Assert.Contains("""<c r="C2" s="2"><v>-5.00</v></c>""", rewritten);
            Assert.Contains("""<c r="D2" s="2"><v>100.00</v></c>""", rewritten);
        }

        /// <summary>Every value <see cref="WorksheetXml"/> carries in a replaced column, so <c>Rewrite</c> never throws.</summary>
        private static AnonymisationDictionaries DictionariesSeenInWorksheetXml()
        {
            var values = AnonymisationDictionaries.ReplacedColumns.ToDictionary(
                column => column, _ => (IReadOnlyList<string>)Array.Empty<string>(), StringComparer.Ordinal);
            values[FinanzguruColumns.Counterparty] = ["Bakery"];

            return AnonymisationDictionaries.Build([values]);
        }
    }
}
