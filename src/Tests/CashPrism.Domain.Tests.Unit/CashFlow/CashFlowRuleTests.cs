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

    public sealed class IncomeInCents
    {
        [Fact]
        public void Adds_Up_The_Categories_That_Came_Out_Positive()
            => Assert.Equal(1219566, CashFlowRule.IncomeInCents([841607, 377959, -56800]));

        [Fact]
        public void Is_Zero_When_Every_Category_Cost_More_Than_It_Brought_In()
            => Assert.Equal(0, CashFlowRule.IncomeInCents([-56800, -2100]));
    }

    public sealed class SpendingInCents
    {
        [Fact]
        public void Adds_Up_The_Categories_That_Came_Out_Negative_As_A_Positive_Number()
            => Assert.Equal(58900, CashFlowRule.SpendingInCents([841607, -56800, -2100]));

        [Fact]
        public void Leaves_Out_A_Category_That_Came_Out_Even()
            => Assert.Equal(2100, CashFlowRule.SpendingInCents([0, -2100]));
    }

    public sealed class CategorySpendingInCents
    {
        [Fact]
        public void Turns_A_Negative_Sum_Into_Positive_Spending()
            => Assert.Equal(56800, CashFlowRule.CategorySpendingInCents(-56800));

        [Fact]
        public void Counts_Nothing_When_The_Category_Came_Out_Positive()
            => Assert.Equal(0, CashFlowRule.CategorySpendingInCents(377959));
    }
}
