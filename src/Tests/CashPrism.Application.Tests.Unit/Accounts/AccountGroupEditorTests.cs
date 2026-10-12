using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;
using CashPrism.TestSupport.Accounts;

namespace CashPrism.Application.Tests.Unit.Accounts;

public sealed class AccountGroupEditorTests
{
    private const string Giro = "DE98999500006166400707";
    private const string Savings = "DE33999500006361627181";
    private const string Loan = "DE98999500006988950813";

    public sealed class ReadAsync
    {
        private readonly FakeAccountBalanceReader balances = new();
        private readonly FakeAccountGroupStore store = new();

        private Task<IReadOnlyList<AccountGroupSummary>> Read()
            => new AccountGroupEditor(balances, store).ReadAsync("Konten");

        [Fact]
        public async Task Starts_With_A_Single_Group_Named_By_The_Caller()
        {
            var groups = await Read();

            Assert.Equal(["Konten"], groups.Select(group => group.Name));
        }

        [Fact]
        public async Task Stores_The_Group_It_Started()
        {
            var groups = await Read();

            Assert.Equal(groups[0].Id, Assert.Single(store.Groups).Id);
        }

        [Fact]
        public async Task Shows_The_Groups_In_Their_Order()
        {
            store.AddGroup("Eigene");
            store.AddGroup("Gemeinsam");

            var groups = await Read();

            Assert.Equal(["Eigene", "Gemeinsam"], groups.Select(group => group.Name));
        }

        [Fact]
        public async Task Shows_The_Accounts_Of_A_Group_In_Their_Order()
        {
            balances.Add("Girokonto", 460_373, Giro);
            balances.Add("Tagesgeld", 2_084_227, Savings);
            store.AddGroup("Eigene", Giro, Savings);

            var groups = await Read();

            Assert.Equal(["Girokonto", "Tagesgeld"], groups[0].Accounts.Select(account => account.AccountName));
        }

        [Fact]
        public async Task Puts_A_New_Account_At_The_End_Of_The_Topmost_Group()
        {
            balances.Add("Girokonto", 460_373, Giro);
            balances.Add("Tagesgeld", 2_084_227, Savings);
            store.AddGroup("Eigene", Giro);
            store.AddGroup("Gemeinsam");

            var groups = await Read();

            Assert.Equal(["Girokonto", "Tagesgeld"], groups[0].Accounts.Select(account => account.AccountName));
        }

        [Fact]
        public async Task Places_New_Accounts_By_Balance()
        {
            balances.Add("Immobiliendarlehen", -15_112_000, Loan);
            balances.Add("Girokonto", 460_373, Giro);
            balances.Add("Tagesgeld", 2_084_227, Savings);

            var groups = await Read();

            Assert.Equal(
                ["Tagesgeld", "Girokonto", "Immobiliendarlehen"],
                groups[0].Accounts.Select(account => account.AccountName));
        }

        [Fact]
        public async Task Does_Not_Store_The_Place_Of_A_New_Account()
        {
            balances.Add("Girokonto", 460_373, Giro);
            store.AddGroup("Eigene");

            await Read();

            Assert.Empty(store.Assignments);
        }

        [Fact]
        public async Task Leaves_Out_An_Account_The_Latest_Export_Does_Not_Carry()
        {
            balances.Add("Girokonto", 460_373, Giro);
            store.AddGroup("Eigene", Savings, Giro);

            var groups = await Read();

            Assert.Equal([Giro], groups[0].Accounts.Select(account => account.AccountReference));
        }

        [Fact]
        public async Task Sums_The_Balances_Of_Each_Group()
        {
            balances.Add("Girokonto", 460_373, Giro);
            balances.Add("Tagesgeld", 2_084_227, Savings);
            balances.Add("Immobiliendarlehen", -15_112_000, Loan);
            store.AddGroup("Eigene", Giro, Savings);
            store.AddGroup("Kredite", Loan);
            store.AddGroup("Leer");

            var groups = await Read();

            Assert.Equal([2_544_600, -15_112_000, 0], groups.Select(group => group.SumInCents));
        }
    }

    public sealed class ChangeAsync
    {
        private readonly FakeAccountBalanceReader balances = new();
        private readonly FakeAccountGroupStore store = new();

        private Task<AccountGroupingOutcome> Change(Func<AccountGrouping, AccountGroupingOutcome> change)
            => new AccountGroupEditor(balances, store).ChangeAsync(change);

        [Fact]
        public async Task Stores_A_Change_That_Was_Made()
        {
            var id = store.AddGroup("Eigene");

            await Change(grouping => grouping.Rename(id, "Meine"));

            Assert.Equal("Meine", Assert.Single(store.Groups).Name);
        }

        [Fact]
        public async Task Does_Not_Store_A_Change_That_Was_Refused()
        {
            var id = store.AddGroup("Eigene");

            var outcome = await Change(grouping => grouping.Delete(id));

            Assert.Equal(AccountGroupingOutcome.LastGroup, outcome);
            Assert.Equal(0, store.Saves);
        }

        [Fact]
        public async Task Hands_The_Change_The_Accounts_Already_Placed()
        {
            balances.Add("Girokonto", 460_373, Giro);
            store.AddGroup("Eigene");
            var shared = store.AddGroup("Gemeinsam");

            var outcome = await Change(grouping => grouping.Assign(Giro, shared));

            Assert.Equal(AccountGroupingOutcome.Done, outcome);
            Assert.Equal(shared, Assert.Single(store.Assignments).GroupId);
        }

        [Fact]
        public async Task Stores_The_Place_Of_A_New_Account_With_The_Change()
        {
            balances.Add("Girokonto", 460_373, Giro);
            var own = store.AddGroup("Eigene");
            var shared = store.AddGroup("Gemeinsam");

            await Change(grouping => grouping.MoveUp(shared));

            Assert.Equal(own, Assert.Single(store.Assignments).GroupId);
        }

        [Fact]
        public async Task Reports_A_Missing_Group_When_None_Is_Stored()
        {
            var outcome = await Change(grouping => grouping.Rename(Guid.NewGuid(), "Meine"));

            Assert.Equal(AccountGroupingOutcome.GroupNotFound, outcome);
            Assert.Equal(0, store.Saves);
        }
    }
}
