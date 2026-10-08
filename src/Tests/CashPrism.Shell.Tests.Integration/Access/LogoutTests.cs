using System.Net;
using CashPrism.Web.Access;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// The logout form in the drawer, posted the way a browser posts it: from a
/// page, with that page's antiforgery token.
/// </summary>
public sealed class LogoutTests : IDisposable
{
    private readonly CashPrismWebApplicationFactory factory = new();

    public void Dispose() => factory.Dispose();

    private static Task<HttpResponseMessage> LogoutAsync(HttpClient client)
        => HtmlForm.SubmitAsync(client, "/", new Dictionary<string, string>(), action: AccessPaths.Logout);

    [Fact]
    public async Task Sends_The_Person_To_The_Login()
    {
        using var client = await factory.CreateSignedInClientAsync();

        using var response = await LogoutAsync(client);

        Assert.Equal(AccessPaths.Login, response.Target());
    }

    [Fact]
    public async Task Ends_The_Login()
    {
        using var client = await factory.CreateSignedInClientAsync();
        using var logout = await LogoutAsync(client);

        using var response = await client.GetAsync("/");

        Assert.Equal(AccessPaths.Login, response.TargetPath());
    }

    [Fact]
    public async Task Refuses_A_Post_Without_The_Token()
    {
        using var client = await factory.CreateSignedInClientAsync();
        using var content = new FormUrlEncodedContent([]);

        using var response = await client.PostAsync(AccessPaths.Logout, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
