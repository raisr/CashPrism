using System.Reflection;
using CashPrism.Web.Access;
using CashPrism.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Every page the web layer has, asked for without a login. The pages are
/// read from the assembly, so a page added later is covered without anyone
/// remembering to list it.
/// </summary>
public sealed class RouteProtectionTests : IClassFixture<CashPrismWebApplicationFactory>, IAsyncLifetime
{
    private readonly CashPrismWebApplicationFactory factory;

    public RouteProtectionTests(CashPrismWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    private static IEnumerable<Type> Pages()
        => typeof(AppVersion).Assembly.GetTypes()
            .Where(type => type.GetCustomAttributes<RouteAttribute>().Any());

    /// <summary>Every route of a page that does not declare itself anonymous.</summary>
    public static TheoryData<string> ProtectedRoutes()
        => [.. Pages()
            .Where(page => page.GetCustomAttribute<AllowAnonymousAttribute>() is null)
            .SelectMany(page => page.GetCustomAttributes<RouteAttribute>())
            .Select(route => route.Template)];

    // A login page is set up before the routes are walked: on a fresh database
    // every request goes to the setup instead, which would prove less.
    public Task InitializeAsync() => factory.SetPasswordAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [MemberData(nameof(ProtectedRoutes))]
    public async Task Sends_A_Request_Without_A_Login_To_The_Login(string route)
    {
        using var client = factory.CreateClientThatStops();

        using var response = await client.GetAsync(route);

        Assert.Equal(AccessPaths.Login, response.TargetPath());
    }

    [Fact]
    public void Leaves_Only_The_Login_And_The_Setup_Anonymous()
    {
        var anonymous = Pages()
            .Where(page => page.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
            .SelectMany(page => page.GetCustomAttributes<RouteAttribute>())
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal);

        Assert.Equal([AccessPaths.Login, AccessPaths.Setup], anonymous);
    }

    [Fact]
    public async Task Refuses_To_Open_A_Circuit_Without_A_Login()
    {
        using var client = factory.CreateClientThatStops();
        using var content = new StringContent(string.Empty);

        using var response = await client.PostAsync("/_blazor/negotiate?negotiateVersion=1", content);

        Assert.False(response.IsSuccessStatusCode, $"The hub answered {response.StatusCode}.");
    }
}
