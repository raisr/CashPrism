using System.Net;
using CashPrism.Application.Access;
using CashPrism.Infrastructure.Persistence;
using CashPrism.Web.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Setting the first password through the page, against the real database
/// and the setup code the host made for this start.
/// </summary>
public sealed class SetupPageTests
{
    private const string AGoodPassword = "korrekt pferd batterie";

    private static Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        string code,
        string password,
        string? repeat = null)
        => HtmlForm.SubmitAsync(client, AccessPaths.Setup, new Dictionary<string, string>
        {
            ["Input.Code"] = code,
            ["Input.Password"] = password,
            ["Input.Repeat"] = repeat ?? password,
        });

    private static string TheCode(CashPrismWebApplicationFactory factory)
        => factory.Services.GetRequiredService<SetupCode>().Display;

    private static async Task<string?> StoredHashAsync(CashPrismWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CashPrismDbContext>();

        return await context.Credentials.Select(credential => credential.PasswordHash).SingleOrDefaultAsync();
    }

    public sealed class OnAFreshDatabase : IDisposable
    {
        private readonly CashPrismWebApplicationFactory factory = new();

        public void Dispose() => factory.Dispose();

        [Fact]
        public async Task Sends_The_Start_Page_To_The_Setup()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await client.GetAsync("/");

            Assert.Equal(AccessPaths.Setup, response.Target());
        }

        [Fact]
        public async Task Sends_The_Login_Page_To_The_Setup()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await client.GetAsync(AccessPaths.Login);

            Assert.Equal(AccessPaths.Setup, response.Target());
        }

        [Fact]
        public async Task Refuses_A_Wrong_Code_And_Stores_Nothing()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await SubmitAsync(client, "2222-2222-2222", AGoodPassword);

            var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            Assert.Contains("Der Einrichtungscode stimmt nicht.", html, StringComparison.Ordinal);
            Assert.Null(await StoredHashAsync(factory));
        }

        [Fact]
        public async Task Refuses_A_Password_Under_Ten_Characters()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await SubmitAsync(client, TheCode(factory), "123456789");

            var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            Assert.Contains("Das Passwort braucht mindestens 10 Zeichen.", html, StringComparison.Ordinal);
            Assert.Null(await StoredHashAsync(factory));
        }

        [Fact]
        public async Task Refuses_Two_Passwords_That_Differ()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await SubmitAsync(client, TheCode(factory), AGoodPassword, "korrekt pferd batteriE");

            Assert.Null(await StoredHashAsync(factory));
        }

        [Fact]
        public async Task Sends_The_Person_To_The_Login_With_The_Right_Code()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await SubmitAsync(client, TheCode(factory), AGoodPassword);

            Assert.Equal(AccessPaths.LoginAfterSetup, response.Target());
        }

        [Fact]
        public async Task Stores_A_Hash_That_Is_Not_The_Password()
        {
            using var client = factory.CreateClientThatStops();

            using var response = await SubmitAsync(client, TheCode(factory), AGoodPassword);

            var stored = await StoredHashAsync(factory);
            Assert.NotNull(stored);
            Assert.DoesNotContain(AGoodPassword, stored, StringComparison.Ordinal);
        }
    }

    public sealed class OnceAPasswordIsSet : IDisposable
    {
        private readonly CashPrismWebApplicationFactory factory = new();

        public void Dispose() => factory.Dispose();

        [Fact]
        public async Task Sends_The_Setup_Page_To_The_Login()
        {
            await factory.SetPasswordAsync();
            using var client = factory.CreateClientThatStops();

            using var response = await client.GetAsync(AccessPaths.Setup);

            Assert.Equal(AccessPaths.Login, response.Target());
        }

        [Fact]
        public async Task Sends_The_Start_Page_To_The_Login()
        {
            await factory.SetPasswordAsync();
            using var client = factory.CreateClientThatStops();

            using var response = await client.GetAsync("/");

            Assert.Equal(AccessPaths.Login, response.TargetPath());
        }
    }
}
