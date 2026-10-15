using CashPrism.Web.Access;
using CashPrism.Web.Preferences;

namespace CashPrism.Shell.Tests.Integration.Preferences;

/// <summary>
/// The stored preferences in the first response, read by the host from the
/// cookie: on a static page, and on the prerender of an interactive one.
/// </summary>
public sealed class PreferenceCookieTests : IDisposable
{
    private readonly CashPrismWebApplicationFactory factory = new();

    public void Dispose() => factory.Dispose();

    private static async Task<string> GetAsync(HttpClient client, string path, string? preferences)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (preferences is not null)
        {
            request.Headers.Add("Cookie", $"{CookiePreferenceStore.CookieName}={preferences}");
        }

        using var response = await client.SendAsync(request);

        return await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task Renders_The_Login_Page_Dark_When_Dark_Was_Chosen()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClient();

        var page = await GetAsync(client, AccessPaths.Login, "theme=dark");

        Assert.Contains("<html lang=\"de\" data-theme=\"dark\">", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Renders_The_Login_Page_Light_When_Light_Was_Chosen()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClient();

        var page = await GetAsync(client, AccessPaths.Login, "theme=light");

        Assert.Contains("<html lang=\"de\" data-theme=\"light\">", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Leaves_The_Theme_Of_The_Login_Page_To_The_System_Without_A_Choice()
    {
        await factory.SetPasswordAsync();
        using var client = factory.CreateClient();

        var page = await GetAsync(client, AccessPaths.Login, preferences: null);

        Assert.Contains("<html lang=\"de\">", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Prerenders_The_Start_Page_Dark_With_The_Collapsed_Drawer()
    {
        using var client = await factory.CreateSignedInClientAsync();

        var page = await GetAsync(client, "/", "theme=dark&sidebar=collapsed");

        Assert.Equal(
            (true, true),
            (page.Contains("data-theme=\"dark\"", StringComparison.Ordinal),
             page.Contains("cp-shell--rail", StringComparison.Ordinal)));
    }
}
