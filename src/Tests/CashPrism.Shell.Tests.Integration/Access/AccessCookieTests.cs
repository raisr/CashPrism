using System.Net;
using CashPrism.Application.Access;
using CashPrism.Domain.Access;
using CashPrism.Web.Access;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// The login cookie against a password that changes underneath it, with the
/// real database and the validation the host wired.
/// </summary>
public sealed class AccessCookieTests
{
    /// <summary>Changes the stored credential directly, the way a change or a reset leaves it.</summary>
    private static async Task ChangeCredentialAsync(
        CashPrismWebApplicationFactory factory,
        Action<Credential> change)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICredentialStore>();
        var credential = await store.GetAsync();

        change(credential!);
        await store.UpdateAsync(credential!);
    }

    public sealed class Configure : IDisposable
    {
        private readonly CashPrismWebApplicationFactory factory = new();

        public void Dispose() => factory.Dispose();

        [Fact]
        public async Task Keeps_A_Login_While_The_Password_Is_Unchanged()
        {
            using var client = await factory.CreateSignedInClientAsync();

            using var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Rejects_A_Cookie_Issued_Before_The_Password_Changed()
        {
            using var client = await factory.CreateSignedInClientAsync();
            await ChangeCredentialAsync(
                factory, credential => credential.SetPassword("a-hash-set-elsewhere", DateTimeOffset.UtcNow));

            using var response = await client.GetAsync("/");

            Assert.Equal(AccessPaths.Login, response.TargetPath());
        }

        [Fact]
        public async Task Rejects_A_Cookie_Issued_Before_A_Reset()
        {
            using var client = await factory.CreateSignedInClientAsync();
            await ChangeCredentialAsync(factory, credential => credential.Reset());

            using var response = await client.GetAsync("/");

            Assert.Equal(AccessPaths.Setup, response.TargetPath());
        }
    }
}
