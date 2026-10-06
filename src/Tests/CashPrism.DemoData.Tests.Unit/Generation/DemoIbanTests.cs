using CashPrism.DemoData.Generation;

namespace CashPrism.DemoData.Tests.Unit.Generation;

public sealed class DemoIbanTests
{
    public sealed class German
    {
        /// <summary>The example IBAN of the German banking industry, a known-good check number.</summary>
        [Fact]
        public void Computes_The_Check_Number_Of_A_Known_Iban()
            => Assert.Equal("DE89370400440532013000", DemoIban.German("370400440532013000"));
    }

    public sealed class For
    {
        [Fact]
        public void Gives_A_Valid_German_Iban_Of_22_Characters()
        {
            var iban = DemoIban.For("Jana Beispiel");

            Assert.Matches("^DE[0-9]{20}$", iban);
            Assert.True(DemoIban.IsValid(iban));
        }

        [Fact]
        public void Gives_The_Same_Iban_For_The_Same_Holder()
            => Assert.Equal(DemoIban.For("Jana Beispiel"), DemoIban.For("Jana Beispiel"));
    }

    public sealed class CreditorId
    {
        /// <summary>The example creditor identifier the Bundesbank publishes.</summary>
        [Fact]
        public void Computes_The_Check_Number_Of_A_Known_Creditor_Id()
            => Assert.Equal("DE98ZZZ09999999999", DemoIban.CreditorId("09999999999"));
    }

    public sealed class IsValid
    {
        [Fact]
        public void Rejects_A_Wrong_Check_Number()
            => Assert.False(DemoIban.IsValid("DE88370400440532013000"));
    }
}
