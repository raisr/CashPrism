using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Tests.Unit.Anonymisation;

public sealed class SyntheticValuesSelfCheckTests
{
    public sealed class FindKeptColumns
    {
        private static readonly IReadOnlyDictionary<string, string> ColumnLetters = new Dictionary<string, string>
        {
            [FinanzguruColumns.BookingDate] = "A",
            [FinanzguruColumns.Amount] = "D",
            [FinanzguruColumns.Balance] = "E",
        };

        private static readonly string Input = Worksheet(date: "46270.0", amount: "-12.5", balance: "0.0");

        [Fact]
        public void Finds_Nothing_When_Every_Value_Changed()
            => Assert.Empty(SyntheticValuesSelfCheck.FindKeptColumns(
                Input, Worksheet(date: "36892.0", amount: "-7.9", balance: "0.0"), ColumnLetters));

        [Fact]
        public void Names_A_Column_That_Still_Carries_Its_Real_Value_However_It_Is_Printed()
            => Assert.Equal(
                [FinanzguruColumns.Amount],
                SyntheticValuesSelfCheck.FindKeptColumns(
                    Input, Worksheet(date: "36892.0", amount: "-12.50", balance: "0.0"), ColumnLetters));

        [Fact]
        public void Lets_A_Zero_Stay_Zero()
            => Assert.DoesNotContain(
                FinanzguruColumns.Balance,
                SyntheticValuesSelfCheck.FindKeptColumns(
                    Input, Worksheet(date: "36892.0", amount: "-7.9", balance: "0.0"), ColumnLetters));

        private static string Worksheet(string date, string amount, string balance)
            => """<worksheet><sheetData>"""
                + """<row r="1"><c r="A1" t="inlineStr"><is><t>Buchungstag</t></is></c></row>"""
                + """<row r="2">"""
                + $"""<c r="A2" s="5" t="n"><v>{date}</v></c>"""
                + $"""<c r="D2" s="7" t="n"><v>{amount}</v></c>"""
                + $"""<c r="E2" s="7" t="n"><v>{balance}</v></c>"""
                + """</row></sheetData></worksheet>""";
    }
}
