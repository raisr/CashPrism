using CashPrism.Domain.Bookings;
using CashPrism.Domain.CashFlow;
using CashPrism.TestSupport.Bookings;

namespace CashPrism.Domain.Tests.Unit.CashFlow;

public sealed class CashFlowRuleTests
{
    private const string AnOriginalFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    public sealed class Counts
    {
        private static readonly Func<Booking, bool> Rule = CashFlowRule.Counts.Compile();

        [Fact]
        public void Counts_An_Ordinary_Booking()
            => Assert.True(Rule(TestBookings.Create()));

        [Fact]
        public void Leaves_Out_A_Transfer_Between_Own_Accounts()
            => Assert.False(Rule(TestBookings.Create(isTransfer: true)));

        [Fact]
        public void Leaves_Out_The_Original_Of_A_Split_Booking()
            => Assert.False(Rule(TestBookings.Create(splitRole: SplitRole.Original)));

        [Fact]
        public void Counts_A_Part_Of_A_Split_Booking()
            => Assert.True(Rule(TestBookings.Create(
                splitRole: SplitRole.Part,
                originalFingerprint: AnOriginalFingerprint)));

        [Fact]
        public void Counts_The_Remainder_Of_A_Split_Booking()
            => Assert.True(Rule(TestBookings.Create(
                splitRole: SplitRole.Remainder,
                originalFingerprint: AnOriginalFingerprint)));

        [Fact]
        public void Counts_A_Booking_Excluded_From_The_Disposable_Income()
            => Assert.True(Rule(TestBookings.Create(isExcludedFromDisposableIncome: true)));
    }

    public sealed class ToEntry
    {
        private static readonly Func<Booking, CashFlowEntry> Rule = CashFlowRule.ToEntry.Compile();

        [Fact]
        public void Counts_A_Positive_Amount_As_Income()
        {
            var entry = Rule(TestBookings.Create(amountInCents: 408250));

            Assert.Equal((408250L, 0L), (entry.IncomeInCents, entry.SpendingInCents));
        }

        [Fact]
        public void Counts_A_Negative_Amount_As_Spending()
        {
            var entry = Rule(TestBookings.Create(amountInCents: -6317));

            Assert.Equal((0L, 6317L), (entry.IncomeInCents, entry.SpendingInCents));
        }

        [Fact]
        public void Counts_A_Zero_Amount_As_Neither()
        {
            var entry = Rule(TestBookings.Create(amountInCents: 0));

            Assert.Equal((0L, 0L), (entry.IncomeInCents, entry.SpendingInCents));
        }

        [Fact]
        public void Carries_The_Date_And_The_Main_Category()
        {
            var bookedOn = new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Unspecified);

            var entry = Rule(TestBookings.Create(bookedOn: bookedOn, category: "Wohnen"));

            Assert.Equal((bookedOn, "Wohnen"), (entry.BookedOn, entry.Category));
        }
    }
}
