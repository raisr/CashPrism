using System.Net;
using CashPrism.Web.Access;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Changing the password through the page, against the real database, the
/// real hasher and the cookie the host hands out.
/// </summary>
public sealed class ChangePasswordPageTests : IDisposable
{
    private const string TheNewPassword = "heftklammer pferd batterie";

    private readonly CashPrismWebApplicationFactory factory = new();

    public void Dispose() => factory.Dispose();

    private static Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        string current,
        string password,
        string? repeat = null)
        => HtmlForm.SubmitAsync(
            client,
            AccessPaths.ChangePassword,
            new Dictionary<string, string>
            {
                ["Input.Current"] = current,
                ["Input.Password"] = password,
                ["Input.Repeat"] = repeat ?? password,
            });

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string password)
        => HtmlForm.SubmitAsync(
            client, AccessPaths.Login, new Dictionary<string, string> { ["Input.Password"] = password });

    private static async Task<string> TextOfAsync(HttpResponseMessage response)
        => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

    [Fact]
    public async Task Refuses_A_Wrong_Current_Password()
    {
        using var client = await factory.CreateSignedInClientAsync();

        using var response = await SubmitAsync(client, "falsches pferd batterie", TheNewPassword);

        Assert.Contains("Das bisherige Passwort stimmt nicht.", await TextOfAsync(response), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Keeps_The_Current_Password_After_A_Wrong_One()
    {
        using var client = await factory.CreateSignedInClientAsync();
        using var change = await SubmitAsync(client, "falsches pferd batterie", TheNewPassword);
        using var anotherDevice = factory.CreateClientThatStops();

        using var response = await LoginAsync(anotherDevice, CashPrismWebApplicationFactory.Password);

        Assert.Equal(ReturnUrl.Fallback, response.Target());
    }

    [Fact]
    public async Task Refuses_A_New_Password_Under_Ten_Characters()
    {
        using var client = await factory.CreateSignedInClientAsync();

        using var response = await SubmitAsync(client, CashPrismWebApplicationFactory.Password, "123456789");

        Assert.Contains("Das Passwort braucht mindestens 10 Zeichen.", await TextOfAsync(response), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Refuses_Two_New_Passwords_That_Differ()
    {
        using var client = await factory.CreateSignedInClientAsync();

        using var response = await SubmitAsync(
            client, CashPrismWebApplicationFactory.Password, TheNewPassword, "heftklammer pferd batteriE");

        Assert.Contains("Die beiden Passwörter stimmen nicht überein.", await TextOfAsync(response), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Sends_The_Person_To_The_Login_After_A_Change()
    {
        using var client = await factory.CreateSignedInClientAsync();

        using var response = await SubmitAsync(client, CashPrismWebApplicationFactory.Password, TheNewPassword);

        Assert.Equal(AccessPaths.LoginAfterChange, response.Target());
    }

    [Fact]
    public async Task Ends_The_Login_Of_This_Device()
    {
        using var client = await factory.CreateSignedInClientAsync();
        using var change = await SubmitAsync(client, CashPrismWebApplicationFactory.Password, TheNewPassword);

        using var response = await client.GetAsync("/");

        Assert.Equal(AccessPaths.Login, response.TargetPath());
    }

    [Fact]
    public async Task Lets_The_New_Password_Log_In()
    {
        using var client = await factory.CreateSignedInClientAsync();
        using var change = await SubmitAsync(client, CashPrismWebApplicationFactory.Password, TheNewPassword);
        using var anotherDevice = factory.CreateClientThatStops();

        using var response = await LoginAsync(anotherDevice, TheNewPassword);

        Assert.Equal(ReturnUrl.Fallback, response.Target());
    }

    [Fact]
    public async Task Refuses_The_Old_Password_After_A_Change()
    {
        using var client = await factory.CreateSignedInClientAsync();
        using var change = await SubmitAsync(client, CashPrismWebApplicationFactory.Password, TheNewPassword);
        using var anotherDevice = factory.CreateClientThatStops();

        using var response = await LoginAsync(anotherDevice, CashPrismWebApplicationFactory.Password);

        Assert.Contains("Das Passwort stimmt nicht.", await TextOfAsync(response), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Says_On_The_Login_That_The_Password_Was_Changed()
    {
        using var client = factory.CreateClientThatStops();
        await factory.SetPasswordAsync();

        var html = WebUtility.HtmlDecode(await client.GetStringAsync(AccessPaths.LoginAfterChange));

        Assert.Contains("Passwort geändert", html, StringComparison.Ordinal);
    }
}
