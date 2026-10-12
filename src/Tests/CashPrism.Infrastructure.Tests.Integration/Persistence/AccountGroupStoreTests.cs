using CashPrism.Domain.Accounts;
using CashPrism.Infrastructure.Persistence;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class AccountGroupStoreTests
{
    private const string Giro = "DE02120300000000202051";
    private const string Savings = "DE89370400440532013000";

    private static readonly Guid Own = new("0f9a1c3e-0000-4000-8000-000000000001");
    private static readonly Guid Shared = new("0f9a1c3e-0000-4000-8000-000000000002");

    private static AccountGrouping TwoGroups()
    {
        var grouping = AccountGrouping.Start(Own, "Eigene");
        grouping.Add(Shared, "Gemeinsam");
        grouping.Place([Giro, Savings]);
        grouping.Assign(Savings, Shared);

        return grouping;
    }

    private static async Task StoreAsync(ThrowawayDatabase database, AccountGrouping grouping)
    {
        await using var context = database.CreateContext();

        await new AccountGroupStore(context).SaveAsync(grouping);
    }

    private static async Task<AccountGrouping?> ReadBackAsync(ThrowawayDatabase database)
    {
        await using var context = database.CreateContext();

        return await new AccountGroupStore(context).LoadAsync();
    }

    public sealed class LoadAsync
    {
        [Fact]
        public async Task Reads_Nothing_From_A_Fresh_Database()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            Assert.Null(await ReadBackAsync(database));
        }
    }

    public sealed class SaveAsync
    {
        [Fact]
        public async Task Round_Trips_The_Groups_In_Their_Order()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var grouping = TwoGroups();
            grouping.MoveUp(Shared);

            await StoreAsync(database, grouping);

            var loaded = await ReadBackAsync(database);
            Assert.Equal(
                [(Shared, "Gemeinsam"), (Own, "Eigene")],
                loaded!.Groups.Select(group => (group.Id, group.Name)));
        }

        [Fact]
        public async Task Round_Trips_The_Accounts_Of_Each_Group()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await StoreAsync(database, TwoGroups());

            var loaded = await ReadBackAsync(database);
            Assert.Equal([Giro], loaded!.AccountsIn(Own));
            Assert.Equal([Savings], loaded.AccountsIn(Shared));
        }

        [Fact]
        public async Task Replaces_What_Was_Stored()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, TwoGroups());
            var grouping = (await ReadBackAsync(database))!;
            grouping.Assign(Savings, Own);
            grouping.Delete(Shared);

            await StoreAsync(database, grouping);

            var loaded = await ReadBackAsync(database);
            Assert.Equal([Own], loaded!.Groups.Select(group => group.Id));
            Assert.Equal([Giro, Savings], loaded.AccountsIn(Own));
        }

        [Fact]
        public async Task Keeps_A_Hidden_Account()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var grouping = TwoGroups();
            grouping.Place([Giro]);

            await StoreAsync(database, grouping);

            var loaded = await ReadBackAsync(database);
            Assert.Equal([Savings], loaded!.AccountsIn(Shared));
        }
    }
}
