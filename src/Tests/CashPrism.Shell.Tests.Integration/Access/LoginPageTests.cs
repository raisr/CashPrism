using System.Net;
using CashPrism.Web.Access;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Logging in through the page, against the real database, the real hasher
/// and the cookie the host hands out.
/// </summary>
public sealed class LoginPageTests : IDisposable
{
    private readonly CashPrismWebApplicationFactory factory = new();

    public void Dispose() => factory.Dispose();

    private static Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        string password,
        bool rememberMe = false,
        string path = AccessPaths.Login)
    {
        var fields = new Dictionary<string, string> { ["Input.Password"] = password };

        if (rememberMe)
        {
            fields["Input.RememberMe"] = "true";
        }

        return HtmlForm.SubmitAsync(client, path, fields);
    }

    /// <summary>The <c>Set-Cookie</c> header that carries the login, or <c>null</c>.</summary>
    private static string? LoginCookie(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.FirstOrDefault(cookie => cookie.StartsWith($"{AccessCookie.Name}=", StringComparison.Ordinal))
            : null;

    [Fact]
    public async Task Sends_The_Person_To_The_Start_Page_With_The_Right_Password()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClientThatStops();

        using var response = await SubmitAsync(client, CashPrismWebApplicationFactory.Password);

        Assert.Equal(ReturnUrl.Fallback, response.Target());
    }

    [Fact]
    public async Task Sends_The_Person_Where_They_Were_Going()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClientThatStops();

        using var response = await SubmitAsync(
            client, CashPrismWebApplicationFactory.Password, path: $"{AccessPaths.Login}?ReturnUrl=%2Fbookings");

        Assert.Equal("/bookings", response.Target());
    }

    [Fact]
    public async Task Does_Not_Send_The_Person_To_Another_Site()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClientThatStops();

        using var response = await SubmitAsync(
            client, CashPrismWebApplicationFactory.Password, path: $"{AccessPaths.Login}?ReturnUrl=%2F%2Fexample.org");

        Assert.Equal(ReturnUrl.Fallback, response.Target());
    }

    [Fact]
    public async Task Refuses_A_Wrong_Password_Without_A_Cookie()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClientThatStops();

        using var response = await SubmitAsync(client, "falsches pferd batterie");

        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("Das Passwort stimmt nicht.", html, StringComparison.Ordinal);
        Assert.Null(LoginCookie(response));
    }

    [Fact]
    public async Task Hands_Out_A_Cookie_That_Ends_With_The_Browser_By_Default()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClientThatStops();

        using var response = await SubmitAsync(client, CashPrismWebApplicationFactory.Password);

        var cookie = LoginCookie(response);
        Assert.NotNull(cookie);
        Assert.DoesNotContain("expires=", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Hands_Out_A_Cookie_For_Thirty_Days_When_Asked_To_Stay_Logged_In()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClientThatStops();

        using var response = await SubmitAsync(client, CashPrismWebApplicationFactory.Password, rememberMe: true);

        var expires = DateTimeOffset.Parse(
            LoginCookie(response)!.Split("; ").Single(part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase))[8..],
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.InRange(expires - DateTimeOffset.UtcNow, TimeSpan.FromDays(29.9), TimeSpan.FromDays(30));
    }

    [Fact]
    public async Task Opens_The_Start_Page_After_A_Login()
    {
        using var client = await factory.CreateSignedInClientAsync();

        using var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
