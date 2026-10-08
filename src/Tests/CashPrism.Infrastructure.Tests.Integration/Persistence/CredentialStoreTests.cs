using CashPrism.Domain.Access;
using CashPrism.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class CredentialStoreTests
{
    private static readonly DateTimeOffset ASetTime = new(2026, 10, 7, 20, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ALaterTime = new(2026, 10, 8, 20, 0, 0, TimeSpan.Zero);

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

    public sealed class UpdateAsync
    {
        [Fact]
        public async Task Stores_A_Changed_Password()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, "the-old-hash");

            await ChangeAsync(database, credential => credential.SetPassword("the-new-hash", ALaterTime));

            await using var context = database.CreateContext();
            var credential = await new CredentialStore(context).GetAsync();

            Assert.Equal(
                ("the-new-hash", ALaterTime, Credential.FirstGeneration + 1),
                (credential?.PasswordHash, credential?.SetAt, credential?.Generation));
        }

        [Fact]
        public async Task Keeps_A_Reset_Credential_Without_A_Password()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await StoreAsync(database, "a-hash");

            await ChangeAsync(database, credential => credential.Reset());

            await using var context = database.CreateContext();
            var credential = await new CredentialStore(context).GetAsync();

            Assert.Equal(
                (false, Credential.FirstGeneration + 1),
                (credential?.IsPasswordSet, credential?.Generation));
        }

        private static async Task ChangeAsync(ThrowawayDatabase database, Action<Credential> change)
        {
            await using var context = database.CreateContext();
            var store = new CredentialStore(context);
            var credential = await store.GetAsync();

            change(credential!);
            await store.UpdateAsync(credential!);
        }
    }
}
