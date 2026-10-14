using CashPrism.Application.Accounts;
using CashPrism.Application.Bookings;
using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.Bookings;
using CashPrism.TestSupport.CashFlow;

namespace CashPrism.Application.Tests.Unit.Accounts;

public sealed class AccountFlowReaderTests
{
    private const string Giro = "DE98999500006166400707";

    public sealed class ReadAsync
    {
        private static readonly DateOnly CurrentTo = new(2026, 10, 2);

        private readonly FakeBookingReader bookings = new();

        private Task<AccountFlows?> Read(DateOnly? currentTo)
            => new AccountFlowReader(bookings, new FakeCashFlowReader(currentTo)).ReadAsync(Giro);

        [Fact]
        public async Task Answers_Nothing_When_Nothing_Was_Imported()
            => Assert.Null(await Read(currentTo: null));

        [Fact]
        public async Task Asks_For_The_Six_Complete_Months_Of_The_Account_Before_The_Running_One()
        {
            await Read(CurrentTo);

            Assert.Equal(
                (new BookingFilter(AccountReference: Giro), new DateOnly(2026, 4, 1), new DateOnly(2026, 9, 1)),
                bookings.MonthlyFlowsAskedFor);
        }

        [Fact]
        public async Task Shows_A_Month_Without_Bookings_As_Zero()
        {
            bookings.MonthlyFlows.Add(new MonthlyCashFlow(new DateOnly(2026, 5, 1), 250000, 6317));

            var flows = await Read(CurrentTo);

            Assert.Equal(
                [
                    new MonthlyCashFlow(new DateOnly(2026, 4, 1), 0, 0),
                    new MonthlyCashFlow(new DateOnly(2026, 5, 1), 250000, 6317),
                    new MonthlyCashFlow(new DateOnly(2026, 6, 1), 0, 0),
                    new MonthlyCashFlow(new DateOnly(2026, 7, 1), 0, 0),
                    new MonthlyCashFlow(new DateOnly(2026, 8, 1), 0, 0),
                    new MonthlyCashFlow(new DateOnly(2026, 9, 1), 0, 0),
                ],
                flows!.Months);
        }

        [Fact]
        public async Task Counts_The_Thirty_Days_Up_To_The_Date_The_Data_Is_Current_To()
        {
            var flows = await Read(CurrentTo);

            Assert.Equal(new BookingPeriod(new DateOnly(2026, 9, 3), CurrentTo), flows!.Recent);
        }

        [Fact]
        public async Task Sums_The_Account_Over_The_Thirty_Days()
        {
            bookings.Summary = new BookingSummary(7, 250000, 81234);

            var flows = await Read(CurrentTo);

            Assert.Equal(new BookingSummary(7, 250000, 81234), flows!.RecentSummary);
            Assert.Equal(
                [new BookingFilter(AccountReference: Giro, Period: new BookingPeriod(new DateOnly(2026, 9, 3), CurrentTo))],
                bookings.SummaryFilters);
        }

        [Fact]
        public async Task Asks_For_The_Four_Latest_Bookings_Of_The_Account_In_The_Thirty_Days()
        {
            await Read(CurrentTo);

            Assert.Equal(
                [new BookingPageRequest(
                    Skip: 0,
                    Take: 4,
                    Filter: new BookingFilter(AccountReference: Giro, Period: new BookingPeriod(new DateOnly(2026, 9, 3), CurrentTo)))],
                bookings.Requests);
        }

        [Fact]
        public async Task Hands_Out_The_Bookings_The_Page_Carries()
        {
            var latest = bookings.Add(new DateTime(2026, 10, 1));

            var flows = await Read(CurrentTo);

            Assert.Equal([latest], flows!.RecentBookings);
        }
    }
}
