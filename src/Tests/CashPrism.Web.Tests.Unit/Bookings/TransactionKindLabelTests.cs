using CashPrism.Web.Bookings;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// Every booking type a measured export contains, spelled as Finanzguru
/// spells it (see <c>docs/finanzguru-export.md</c>).
/// </summary>
public sealed class TransactionKindLabelTests
{
    public sealed class KeyFor
    {
        public static TheoryData<string, string> KnownKinds => new()
        {
            { "Kartenzahlung", "TransactionKindCardPayment" },
            { "SEPA-Lastschrift", "TransactionKindDirectDebit" },
            { "Ueberweisung", "TransactionKindBankTransfer" },
            { "Dauerauftrag", "TransactionKindStandingOrder" },
            { "Barentnahme", "TransactionKindCashWithdrawal" },
            { "Zinsen/Entgelt", "TransactionKindInterestAndFees" },
            { "Sonstige", "TransactionKindOther" },
        };

        [Theory]
        [MemberData(nameof(KnownKinds))]
        public void Names_The_Resource_Key_Of_A_Known_Kind(string transactionKind, string key)
            => Assert.Equal(key, TransactionKindLabel.KeyFor(transactionKind));

        [Fact]
        public void Has_No_Key_For_A_Kind_It_Does_Not_Know()
            => Assert.Null(TransactionKindLabel.KeyFor("Scheck"));
    }
}
