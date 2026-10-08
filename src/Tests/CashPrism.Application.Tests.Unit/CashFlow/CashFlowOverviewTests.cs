using CashPrism.Application.CashFlow;

namespace CashPrism.Application.Tests.Unit.CashFlow;

public sealed class CashFlowOverviewTests
{
    private static readonly DateOnly October = new(2026, 10, 1);
    private static readonly DateOnly September = new(2026, 9, 1);
    private static readonly DateOnly August = new(2026, 8, 1);

    private static CashFlowOverview Comparing(MonthlyCashFlow september, MonthlyCashFlow august)
        => new(new DateOnly(2026, 10, 1), [august, september, new MonthlyCashFlow(October, 0, 0)], []);

    public sealed class IncomeChange
    {
        [Fact]
        public void Is_The_Change_Against_The_Month_Before_As_A_Fraction()
        {
            var overview = Comparing(new MonthlyCashFlow(September, 408250, 0), new MonthlyCashFlow(August, 400000, 0));

            Assert.Equal(0.020625m, overview.IncomeChange);
        }

        [Fact]
        public void Is_Missing_When_The_Month_Before_Had_No_Income()
        {
            var overview = Comparing(new MonthlyCashFlow(September, 408250, 0), new MonthlyCashFlow(August, 0, 0));

            Assert.Null(overview.IncomeChange);
        }
    }

    public sealed class SpendingChange
    {
        [Fact]
        public void Is_Negative_When_Less_Went_Out()
        {
            var overview = Comparing(new MonthlyCashFlow(September, 0, 150000), new MonthlyCashFlow(August, 0, 200000));

            Assert.Equal(-0.25m, overview.SpendingChange);
        }

        [Fact]
        public void Is_Missing_When_Nothing_Went_Out_In_The_Month_Before()
        {
            var overview = Comparing(new MonthlyCashFlow(September, 0, 150000), new MonthlyCashFlow(August, 0, 0));

            Assert.Null(overview.SpendingChange);
        }
    }

    public sealed class LeftChangeInCents
    {
        [Fact]
        public void Is_The_Difference_Of_What_Was_Left_Even_Across_Zero()
        {
            var overview = Comparing(new MonthlyCashFlow(September, 300000, 250000), new MonthlyCashFlow(August, 300000, 320000));

            Assert.Equal(70000, overview.LeftChangeInCents);
        }
    }
}
