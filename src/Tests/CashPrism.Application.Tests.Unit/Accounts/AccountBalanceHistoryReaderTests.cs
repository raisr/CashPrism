using CashPrism.Application.Accounts;
using CashPrism.TestSupport.Accounts;
using CashPrism.TestSupport.CashFlow;

namespace CashPrism.Application.Tests.Unit.Accounts;

public sealed class AccountBalanceHistoryReaderTests
{
    private const string Giro = "DE98999500006166400707";
    private const string Savings = "DE33999500006361627181";

    public sealed class ReadAsync
    {
        private static readonly DateOnly CurrentTo = new(2026, 10, 1);

        private readonly FakeAccountBalanceReader balances = new();
        private readonly FakeBalanceMovementReader movements = new();

        private Task<IReadOnlyList<AccountBalanceHistory>> Read(DateOnly? currentTo)
            => new AccountBalanceHistoryReader(balances, movements, new FakeCashFlowReader(currentTo)).ReadAsync();

        [Fact]
        public async Task Answers_Nothing_When_Nothing_Was_Imported()
        {
            balances.Add("Girokonto", 100000, Giro);

            Assert.Empty(await Read(currentTo: null));
        }

        [Fact]
        public async Task Gives_Twelve_Month_Ends_And_The_Day_The_Data_Is_Current_To()
        {
            balances.Add("Girokonto", 100000, Giro);
            movements.Add(Giro, new DateOnly(2024, 1, 15), 100000, 100000);

            var history = Assert.Single(await Read(CurrentTo));

            Assert.Equal(
                [
                    new DateOnly(2025, 10, 31), new DateOnly(2025, 11, 30), new DateOnly(2025, 12, 31),
                    new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 31),
                    new DateOnly(2026, 4, 30), new DateOnly(2026, 5, 31), new DateOnly(2026, 6, 30),
                    new DateOnly(2026, 7, 31), new DateOnly(2026, 8, 31), new DateOnly(2026, 9, 30),
                    CurrentTo,
                ],
                history.Points.Select(point => point.Day));
        }

        [Fact]
        public async Task Walks_Back_From_The_Balance_Taken_At_Import()
        {
            balances.Add("Girokonto", 100000, Giro);
            movements.Add(Giro, new DateOnly(2026, 10, 1), -20000, 100000);
            movements.Add(Giro, new DateOnly(2024, 1, 15), 120000, 120000);

            var history = Assert.Single(await Read(CurrentTo));

            Assert.Equal([120000L, 100000L], history.Points.TakeLast(2).Select(point => point.BalanceInCents));
        }

        [Fact]
        public async Task Leaves_Out_The_Months_Before_The_First_Booking()
        {
            balances.Add("Girokonto", 100000, Giro);
            movements.Add(Giro, new DateOnly(2026, 8, 10), 100000, 100000);

            var history = Assert.Single(await Read(CurrentTo));

            Assert.Equal([new DateOnly(2026, 8, 31), new DateOnly(2026, 9, 30), CurrentTo], history.Points.Select(point => point.Day));
        }

        [Fact]
        public async Task Walks_Each_Account_On_Its_Own_Bookings()
        {
            balances.Add("Girokonto", 100000, Giro);
            balances.Add("Tagesgeld", 500000, Savings);
            movements.Add(Giro, new DateOnly(2026, 9, 10), 100000, 100000);
            movements.Add(Savings, new DateOnly(2026, 9, 10), 500000, 500000);

            var histories = await Read(CurrentTo);

            Assert.Equal(
                [(Giro, 100000L), (Savings, 500000L)],
                histories.Select(history => (history.AccountReference, history.Points[^1].BalanceInCents)));
        }

        [Fact]
        public async Task Has_No_Points_For_An_Account_Whose_History_Does_Not_Hold()
        {
            balances.Add("Girokonto", 100000, Giro);
            movements.Add(Giro, new DateOnly(2026, 9, 10), 50000, 99999);

            var history = Assert.Single(await Read(CurrentTo));

            Assert.Equal((false, 0), (history.Holds, history.Points.Count));
        }
    }
}
