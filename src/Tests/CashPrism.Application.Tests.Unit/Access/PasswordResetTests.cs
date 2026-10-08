using CashPrism.Application.Access;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class PasswordResetTests
{
    public sealed class ResetAsync
    {
        [Fact]
        public async Task Drops_The_Password()
        {
            var store = FakeCredentialStore.Holding("a-hash");

            await new PasswordReset(store).ResetAsync();

            Assert.False(store.Credential?.IsPasswordSet);
        }

        [Fact]
        public async Task Raises_The_Generation()
        {
            var store = FakeCredentialStore.Holding("a-hash");

            await new PasswordReset(store).ResetAsync();

            Assert.Equal(Credential.FirstGeneration + 1, store.Credential?.Generation);
        }

        [Fact]
        public async Task Says_That_There_Was_A_Password_To_Drop()
        {
            Assert.True(await new PasswordReset(FakeCredentialStore.Holding("a-hash")).ResetAsync());
        }

        [Fact]
        public async Task Does_Nothing_While_No_Password_Was_Ever_Set()
        {
            var store = new FakeCredentialStore();

            Assert.False(await new PasswordReset(store).ResetAsync());
            Assert.Null(store.Credential);
        }

        [Fact]
        public async Task Does_Nothing_When_The_Password_Was_Reset_Already()
        {
            var store = FakeCredentialStore.HoldingAReset();
            var generation = store.Credential!.Generation;

            Assert.False(await new PasswordReset(store).ResetAsync());
            Assert.Equal(generation, store.Credential.Generation);
        }
    }
}
