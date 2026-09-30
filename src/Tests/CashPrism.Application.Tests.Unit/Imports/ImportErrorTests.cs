using CashPrism.Application.Imports;

namespace CashPrism.Application.Tests.Unit.Imports;

public sealed class ImportErrorTests
{
    public sealed class MissingColumns
    {
        [Fact]
        public void Carries_Every_Column_As_One_Comma_Separated_Argument()
        {
            var error = ImportError.MissingColumns(["Tags", "Split-Typ"]);

            Assert.Equal(["Tags, Split-Typ"], error.Arguments);
        }
    }

    public sealed class NotAnAmount
    {
        [Fact]
        public void Carries_Column_Row_And_Value_In_That_Order()
            => Assert.Equal(
                ["Betrag", "12345", "abc"],
                ImportError.NotAnAmount("Betrag", 12345, "abc").Arguments);
    }

    public sealed class ToStringMethod
    {
        [Fact]
        public void Names_The_Code_And_Every_Argument()
            => Assert.Equal(
                "EmptyValue: Waehrung | 4",
                ImportError.EmptyValue("Waehrung", 4).ToString());
    }
}
