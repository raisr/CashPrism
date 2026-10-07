using CashPrism.Domain.Access;
using CashPrism.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class CredentialStoreTests
{
    private static readonly DateTimeOffset ASetTime = new(2026, 10, 7, 20, 0, 0, TimeSpan.Zero);

    private static async Task StoreAsync(ThrowawayDatabase database, string passwordHash)
    {
        await using var context = database.CreateContext();

        await new CredentialStore(context).AddAsync(new Credential(passwordHash, ASetTime));
    }

    public sealed class GetAsync
    {
        [Fact]
        public async Task Returns_Nothing_While_No_Password_Is_Set()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await using var context = database.CreateContext();

            Assert.Null(await new CredentialStore(context).GetAsync());
        }

        [Fact]
        public async Task Returns_The_Stored_Credential()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, "a-hash");

            await using var context = database.CreateContext();
            var credential = await new CredentialStore(context).GetAsync();

            Assert.Equal(("a-hash", ASetTime), (credential?.PasswordHash, credential?.SetAt));
        }
    }

    public sealed class AddAsync
    {
        [Fact]
        public async Task Refuses_A_Second_Credential()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, "the-first-hash");

            await Assert.ThrowsAsync<DbUpdateException>(() => StoreAsync(database, "a-second-hash"));
        }
    }
}
