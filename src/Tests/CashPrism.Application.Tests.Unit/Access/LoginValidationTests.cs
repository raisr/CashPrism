using CashPrism.Application.Access;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class LoginValidationTests
{
    public sealed class IsCurrentAsync
    {
        [Fact]
        public async Task Accepts_A_Login_Issued_Under_The_Current_Password()
        {
            var validation = new LoginValidation(FakeCredentialStore.Holding("a-hash"));

            Assert.True(await validation.IsCurrentAsync(Credential.FirstGeneration));
        }

        [Fact]
        public async Task Rejects_A_Login_Issued_Before_The_Password_Changed()
        {
            var store = FakeCredentialStore.Holding("the-old-hash");
            store.Credential!.SetPassword("the-new-hash", DateTimeOffset.UnixEpoch);

            Assert.False(await new LoginValidation(store).IsCurrentAsync(Credential.FirstGeneration));
        }

        [Fact]
        public async Task Rejects_Every_Login_After_A_Reset()
        {
            var store = FakeCredentialStore.HoldingAReset();

            Assert.False(await new LoginValidation(store).IsCurrentAsync(store.Credential!.Generation));
        }

        [Fact]
        public async Task Rejects_Every_Login_While_No_Password_Was_Ever_Set()
        {
            var validation = new LoginValidation(new FakeCredentialStore());

            Assert.False(await validation.IsCurrentAsync(Credential.FirstGeneration));
        }
    }
}
