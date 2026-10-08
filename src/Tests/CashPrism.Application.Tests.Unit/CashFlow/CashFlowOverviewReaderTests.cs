using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.CashFlow;

namespace CashPrism.Application.Tests.Unit.CashFlow;

public sealed class CashFlowOverviewReaderTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 1);
    private static readonly DateOnly October = new(2026, 10, 1);
    private static readonly DateOnly September = new(2026, 9, 1);
    private static readonly DateOnly August = new(2026, 8, 1);

    public sealed class ReadAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            var overview = await new CashFlowOverviewReader(new FakeCashFlowReader(currentTo: null)).ReadAsync();

            Assert.Null(overview);
        }

        [Fact]
        public async Task Reads_Twenty_Four_Months_Ending_With_The_Month_The_Export_Was_Taken_In()
        {
            var reader = new FakeCashFlowReader(ExportedOn);

            await new CashFlowOverviewReader(reader).ReadAsync();

            Assert.Equal((new DateOnly(2024, 11, 1), October), reader.MonthsAskedFor);
        }

        [Fact]
        public async Task Fills_A_Month_Without_Bookings_With_Zero()
        {
            var reader = new FakeCashFlowReader(ExportedOn, [new MonthlyCashFlow(September, 400000, 250000)]);

            var overview = await new CashFlowOverviewReader(reader).ReadAsync();

            Assert.Equal(new MonthlyCashFlow(August, 0, 0), overview?.MonthBefore);
        }

        [Fact]
        public async Task Takes_The_Month_Before_The_Running_One_As_The_Last_Complete_Month()
        {
            var september = new MonthlyCashFlow(September, 400000, 250000);
            var reader = new FakeCashFlowReader(ExportedOn, [september, new MonthlyCashFlow(October, 0, 4500)]);

            var overview = await new CashFlowOverviewReader(reader).ReadAsync();

            Assert.Equal(september, overview?.LastCompleteMonth);
        }

        [Fact]
        public async Task Reads_The_Categories_Of_The_Last_Complete_Month()
        {
            var reader = new FakeCashFlowReader(ExportedOn);

            await new CashFlowOverviewReader(reader).ReadAsync();

            Assert.Equal(September, reader.CategoryMonthAskedFor);
        }
    }
}
