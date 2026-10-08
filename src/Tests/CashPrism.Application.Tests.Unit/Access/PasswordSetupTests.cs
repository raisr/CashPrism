using CashPrism.Application.Access;
using CashPrism.TestSupport.Imports;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class PasswordSetupTests
{
    private const string TheCode = "K7QF-M2XP-9HTR";
    private const string AGoodPassword = "korrekt pferd batterie";

    private static readonly DateTimeOffset Now = new(2026, 10, 7, 20, 0, 0, TimeSpan.Zero);

    private static PasswordSetup CreateSetup(FakeCredentialStore store)
        => new(store, new FakePasswordHasher(), new SetupCode("K7QFM2XP9HTR"), new FixedClock(Now));

    public sealed class IsPendingAsync
    {
        [Fact]
        public async Task Is_Pending_While_No_Password_Is_Set()
        {
            Assert.True(await CreateSetup(new FakeCredentialStore()).IsPendingAsync());
        }

        [Fact]
        public async Task Is_Not_Pending_Once_A_Password_Is_Set()
        {
            Assert.False(await CreateSetup(FakeCredentialStore.Holding("a-hash")).IsPendingAsync());
        }

        [Fact]
        public async Task Is_Pending_After_A_Reset()
        {
            Assert.True(await CreateSetup(FakeCredentialStore.HoldingAReset()).IsPendingAsync());
        }
    }

    public sealed class SetAsync
    {
        [Fact]
        public async Task Stores_The_Hash_Of_The_Password_With_The_Right_Code()
        {
            var store = new FakeCredentialStore();

            var outcome = await CreateSetup(store).SetAsync(TheCode, AGoodPassword);

            Assert.Equal(PasswordSetupOutcome.Done, outcome);
            Assert.Equal(FakePasswordHasher.HashOf(AGoodPassword), store.Credential?.PasswordHash);
        }

        [Fact]
        public async Task Records_When_The_Password_Was_Set()
        {
            var store = new FakeCredentialStore();

            await CreateSetup(store).SetAsync(TheCode, AGoodPassword);

            Assert.Equal(Now, store.Credential?.SetAt);
        }

        [Fact]
        public async Task Refuses_A_Wrong_Code_Without_Storing_Anything()
        {
            var store = new FakeCredentialStore();

            var outcome = await CreateSetup(store).SetAsync("K7QF-M2XP-9HTS", AGoodPassword);

            Assert.Equal(PasswordSetupOutcome.WrongCode, outcome);
            Assert.Null(store.Credential);
        }

        [Fact]
        public async Task Refuses_A_Missing_Code()
        {
            var outcome = await CreateSetup(new FakeCredentialStore()).SetAsync(null, AGoodPassword);

            Assert.Equal(PasswordSetupOutcome.WrongCode, outcome);
        }

        [Fact]
        public async Task Refuses_A_Password_Under_Ten_Characters_Without_Storing_Anything()
        {
            var store = new FakeCredentialStore();

            var outcome = await CreateSetup(store).SetAsync(TheCode, "123456789");

            Assert.Equal(PasswordSetupOutcome.TooShort, outcome);
            Assert.Null(store.Credential);
        }

        [Fact]
        public async Task Refuses_The_Right_Code_Once_A_Password_Is_Set()
        {
            var store = FakeCredentialStore.Holding("the-first-hash");

            var outcome = await CreateSetup(store).SetAsync(TheCode, AGoodPassword);

            Assert.Equal(PasswordSetupOutcome.AlreadySet, outcome);
            Assert.Equal("the-first-hash", store.Credential?.PasswordHash);
        }

        [Fact]
        public async Task Sets_The_Password_Again_After_A_Reset()
        {
            var store = FakeCredentialStore.HoldingAReset();

            var outcome = await CreateSetup(store).SetAsync(TheCode, AGoodPassword);

            Assert.Equal(PasswordSetupOutcome.Done, outcome);
            Assert.Equal(FakePasswordHasher.HashOf(AGoodPassword), store.Credential?.PasswordHash);
        }

        [Fact]
        public async Task Keeps_Counting_Generations_After_A_Reset()
        {
            var store = FakeCredentialStore.HoldingAReset();
            var generationAfterReset = store.Credential!.Generation;

            await CreateSetup(store).SetAsync(TheCode, AGoodPassword);

            Assert.Equal(generationAfterReset + 1, store.Credential.Generation);
        }
    }
}
