using CashPrism.Application.Accounts;
using CashPrism.TestSupport.Accounts;

namespace CashPrism.Application.Tests.Unit.Accounts;

public sealed class AccountOverviewReaderTests
{
    public sealed class ReadAsync
    {
        private readonly FakeAccountBalanceReader balances = new();

        private Task<AccountOverview?> Read() => new AccountOverviewReader(balances).ReadAsync();

        [Fact]
        public async Task Answers_Nothing_Without_A_Balance()
            => Assert.Null(await Read());

        [Fact]
        public async Task Lists_The_Highest_Balance_First()
        {
            balances.Add("Girokonto", 460_373, "DE98999500006166400707");
            balances.Add("Tagesgeld", 2_084_227, "DE33999500006361627181");
            balances.Add("Immobiliendarlehen", -15_112_000, "DE98999500006988950813");

            var overview = await Read();

            Assert.Equal(
                ["Tagesgeld", "Girokonto", "Immobiliendarlehen"],
                overview!.Accounts.Select(account => account.AccountName));
        }

        [Fact]
        public async Task Breaks_A_Tie_In_The_Balance_By_The_Name()
        {
            balances.Add("Paynet", 0, "Paynet");
            balances.Add("Altes Sparkonto", 0, "DE42999500008510119110");

            var overview = await Read();

            Assert.Equal(["Altes Sparkonto", "Paynet"], overview!.Accounts.Select(account => account.AccountName));
        }

        [Fact]
        public async Task Totals_The_Balances_A_Loan_Included()
        {
            balances.Add("Girokonto", 460_373, "DE98999500006166400707");
            balances.Add("Immobiliendarlehen", -15_112_000, "DE98999500006988950813");

            var overview = await Read();

            Assert.Equal(-14_651_627, overview!.TotalInCents);
        }
    }
}
