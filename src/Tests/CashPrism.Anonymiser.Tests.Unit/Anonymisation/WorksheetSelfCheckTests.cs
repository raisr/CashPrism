using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class WorksheetSelfCheckTests
{
    public sealed class FindLeakedColumns
    {
        [Fact]
        public void Reports_A_Column_Whose_Output_Still_Carries_An_Original_Value()
        {
            // A deliberately incomplete replacement: the output still shows the
            // raw "Bakery" text in the counterparty column instead of a placeholder.
            const string worksheetXml =
                """<worksheet><sheetData>"""
                + """<row r="1"><c r="A1" t="inlineStr"><is><t>Beguenstigter/Auftraggeber</t></is></c></row>"""
                + """<row r="2"><c r="A2" t="inlineStr"><is><t>Bakery</t></is></c></row>"""
                + """</sheetData></worksheet>""";

            var columnLetters = new Dictionary<string, string> { [FinanzguruColumns.Counterparty] = "A" };
            var dictionaries = AnonymisationDictionaries.Build(
                [OneFileWithCounterparty("Bakery")]);

            var leaked = WorksheetSelfCheck.FindLeakedColumns(worksheetXml, columnLetters, dictionaries);

            Assert.Equal([FinanzguruColumns.Counterparty], leaked);
        }

        [Fact]
        public void A_Properly_Anonymised_Column_Reports_No_Leak()
        {
            const string worksheetXml =
                """<worksheet><sheetData>"""
                + """<row r="1"><c r="A1" t="inlineStr"><is><t>Beguenstigter/Auftraggeber</t></is></c></row>"""
                + """<row r="2"><c r="A2" t="inlineStr"><is><t>Counterparty 01</t></is></c></row>"""
                + """</sheetData></worksheet>""";

            var columnLetters = new Dictionary<string, string> { [FinanzguruColumns.Counterparty] = "A" };
            var dictionaries = AnonymisationDictionaries.Build(
                [OneFileWithCounterparty("Bakery")]);

            var leaked = WorksheetSelfCheck.FindLeakedColumns(worksheetXml, columnLetters, dictionaries);

            Assert.Empty(leaked);
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> OneFileWithCounterparty(string value)
        {
            var values = AnonymisationDictionaries.ReplacedColumns.ToDictionary(
                column => column, _ => (IReadOnlyList<string>)Array.Empty<string>(), StringComparer.Ordinal);

            values[FinanzguruColumns.Counterparty] = [value];

            return values;
        }
    }
}
