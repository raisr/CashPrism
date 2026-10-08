using CashPrism.Application.Access;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class PasswordChangeTests
{
    private const string TheCurrentPassword = "korrekt pferd batterie";
    private const string TheNewPassword = "heftklammer pferd batterie";
    private const string AWrongPassword = "falsches pferd batterie";

    private static FakeCredentialStore HoldingTheCurrentPassword()
        => FakeCredentialStore.Holding(FakePasswordHasher.HashOf(TheCurrentPassword));

    private static PasswordChange CreateChange(FakeCredentialStore store, SteppingClock clock)
    {
        var hasher = new FakePasswordHasher();

        return new PasswordChange(new PasswordCheck(store, hasher, new LoginThrottle(clock)), store, hasher, clock);
    }

    private static async Task FailAsync(PasswordChange change, int times)
    {
        for (var attempt = 0; attempt < times; attempt++)
        {
            await change.ChangeAsync(AWrongPassword, TheNewPassword);
        }
    }

    public sealed class ChangeAsync
    {
        [Fact]
        public async Task Stores_The_Hash_Of_The_New_Password_With_The_Right_Current_One()
        {
            var store = HoldingTheCurrentPassword();

            var result = await CreateChange(store, new SteppingClock()).ChangeAsync(TheCurrentPassword, TheNewPassword);

            Assert.Equal(PasswordChangeOutcome.Done, result.Outcome);
            Assert.Equal(FakePasswordHasher.HashOf(TheNewPassword), store.Credential?.PasswordHash);
        }

        [Fact]
        public async Task Raises_The_Generation()
        {
            var store = HoldingTheCurrentPassword();

            await CreateChange(store, new SteppingClock()).ChangeAsync(TheCurrentPassword, TheNewPassword);

            Assert.Equal(Credential.FirstGeneration + 1, store.Credential?.Generation);
        }

        [Fact]
        public async Task Records_When_The_Password_Was_Set()
        {
            var store = HoldingTheCurrentPassword();
            var clock = new SteppingClock();

            await CreateChange(store, clock).ChangeAsync(TheCurrentPassword, TheNewPassword);

            Assert.Equal(clock.UtcNow, store.Credential?.SetAt);
        }

        [Fact]
        public async Task Refuses_A_Wrong_Current_Password_Without_Changing_Anything()
        {
            var store = HoldingTheCurrentPassword();

            var result = await CreateChange(store, new SteppingClock()).ChangeAsync(AWrongPassword, TheNewPassword);

            Assert.Equal(PasswordChangeOutcome.Rejected, result.Outcome);
            Assert.Equal(FakePasswordHasher.HashOf(TheCurrentPassword), store.Credential?.PasswordHash);
        }

        [Fact]
        public async Task Refuses_A_New_Password_Under_Ten_Characters_Without_Changing_Anything()
        {
            var store = HoldingTheCurrentPassword();

            var result = await CreateChange(store, new SteppingClock()).ChangeAsync(TheCurrentPassword, "123456789");

            Assert.Equal(PasswordChangeOutcome.TooShort, result.Outcome);
            Assert.Equal(FakePasswordHasher.HashOf(TheCurrentPassword), store.Credential?.PasswordHash);
        }

        [Fact]
        public async Task Refuses_The_Right_Current_Password_During_A_Lockout()
        {
            var change = CreateChange(HoldingTheCurrentPassword(), new SteppingClock());
            await FailAsync(change, 5);

            var result = await change.ChangeAsync(TheCurrentPassword, TheNewPassword);

            Assert.Equal(PasswordChangeOutcome.LockedOut, result.Outcome);
        }
    }
}
