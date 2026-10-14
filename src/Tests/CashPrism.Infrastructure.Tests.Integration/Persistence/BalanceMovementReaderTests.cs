using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;
using CashPrism.Domain.Bookings;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Bookings;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The projection the balance history is walked on, against a real SQLite
/// file — and the walk itself over the imported demo export, because only a
/// whole export shows whether the history holds for every account.
/// </summary>
public sealed class BalanceMovementReaderTests
{
    public sealed class ReadAllAsync
    {
        [Fact]
        public async Task Projects_Account_Day_Amount_Split_Role_And_Reported_Balance()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await using (var seed = database.CreateContext())
            {
                seed.Bookings.Add(TestBookings.Create(
                    bookedOn: new DateTime(2026, 9, 30, 23, 15, 0),
                    amountInCents: -6000,
                    accountReference: "DE98999500006166400707",
                    splitRole: SplitRole.Part,
                    originalFingerprint: "0".PadLeft(40, '0'),
                    reportedBalanceInCents: 120000));
                await seed.SaveChangesAsync();
            }

            await using var context = database.CreateContext();

            var movements = await new BalanceMovementReader(context).ReadAllAsync();

            Assert.Equal(
                [new BalanceMovement("DE98999500006166400707", new DateOnly(2026, 9, 30), -6000, SplitRole.Part, 120000)],
                movements);
        }

        [Fact]
        public async Task Gives_Every_Account_Of_The_Demo_Export_A_History_That_Holds()
        {
            await using var database = await DemoSampleDatabase.CreateAsync();
            await using var context = database.CreateContext();
            var reader = new AccountBalanceHistoryReader(
                new AccountBalanceReader(context),
                new BalanceMovementReader(context),
                new CashFlowReader(context));

            var histories = await reader.ReadAsync();

            Assert.Equal(
                [(true, 13), (true, 13), (true, 13), (true, 13), (true, 13), (true, 13), (true, 13)],
                histories.Select(history => (history.Holds, history.Points.Count)));
        }
    }
}
