using CashPrism.Application.Access;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class PasswordCheckTests
{
    private const string ThePassword = "korrekt pferd batterie";
    private const string AWrongPassword = "falsches pferd batterie";

    private static PasswordCheck CreateCheck(LoginThrottle throttle)
        => new(FakeCredentialStore.Holding(FakePasswordHasher.HashOf(ThePassword)), new FakePasswordHasher(), throttle);

    private static async Task FailAsync(PasswordCheck check, int times)
    {
        for (var attempt = 0; attempt < times; attempt++)
        {
            await check.CheckAsync(AWrongPassword);
        }
    }

    public sealed class CheckAsync
    {
        [Fact]
        public async Task Accepts_The_Password()
        {
            var check = CreateCheck(new LoginThrottle(new SteppingClock()));

            var result = await check.CheckAsync(ThePassword);

            Assert.Equal(LoginOutcome.Accepted, result.Outcome);
        }

        [Fact]
        public async Task Hands_Out_The_Generation_Of_The_Password_That_Was_Right()
        {
            var check = CreateCheck(new LoginThrottle(new SteppingClock()));

            var result = await check.CheckAsync(ThePassword);

            Assert.Equal(Credential.FirstGeneration, result.Generation);
        }

        [Fact]
        public async Task Rejects_A_Wrong_Password()
        {
            var check = CreateCheck(new LoginThrottle(new SteppingClock()));

            var result = await check.CheckAsync(AWrongPassword);

            Assert.Equal(LoginOutcome.Rejected, result.Outcome);
        }

        [Fact]
        public async Task Rejects_Every_Password_While_None_Is_Set()
        {
            var check = new PasswordCheck(
                new FakeCredentialStore(), new FakePasswordHasher(), new LoginThrottle(new SteppingClock()));

            var result = await check.CheckAsync(ThePassword);

            Assert.Equal(LoginOutcome.Rejected, result.Outcome);
        }

        [Fact]
        public async Task Rejects_Every_Password_After_A_Reset()
        {
            var check = new PasswordCheck(
                FakeCredentialStore.HoldingAReset(), new FakePasswordHasher(), new LoginThrottle(new SteppingClock()));

            var result = await check.CheckAsync(ThePassword);

            Assert.Equal(LoginOutcome.Rejected, result.Outcome);
        }

        [Fact]
        public async Task Says_When_The_Fifth_Wrong_Password_Started_A_Lockout()
        {
            var clock = new SteppingClock();
            var check = CreateCheck(new LoginThrottle(clock));
            await FailAsync(check, 4);

            var result = await check.CheckAsync(AWrongPassword);

            Assert.Equal(clock.UtcNow + LoginThrottle.FirstLockout, result.LockedUntil);
        }

        [Fact]
        public async Task Refuses_The_Right_Password_For_A_Minute_After_Five_Wrong_Ones()
        {
            var clock = new SteppingClock();
            var check = CreateCheck(new LoginThrottle(clock));
            await FailAsync(check, 5);
            clock.Advance(TimeSpan.FromSeconds(59));

            var result = await check.CheckAsync(ThePassword);

            Assert.Equal(LoginOutcome.LockedOut, result.Outcome);
        }

        [Fact]
        public async Task Accepts_The_Right_Password_Once_The_Lockout_Is_Over()
        {
            var clock = new SteppingClock();
            var check = CreateCheck(new LoginThrottle(clock));
            await FailAsync(check, 5);
            clock.Advance(TimeSpan.FromMinutes(1));

            var result = await check.CheckAsync(ThePassword);

            Assert.Equal(LoginOutcome.Accepted, result.Outcome);
        }
    }
}
