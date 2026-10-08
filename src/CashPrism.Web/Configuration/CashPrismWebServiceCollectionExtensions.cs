using CashPrism.Web.Access;
using CashPrism.Web.Imports;
using CashPrism.Web.Layout;
using CashPrism.Web.Localisation;
using CashPrism.Web.StoredData;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Configuration;

/// <summary>
/// Registers the CashPrism web layer (Blazor components, auth UI, routing) with
/// the application's service container. Called by the host in <c>CashPrism.Shell</c>.
/// </summary>
public static class CashPrismWebServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the CashPrism web UI. Interactivity is
    /// global: every component renders with the interactive server render mode.
    /// </summary>
    /// <remarks>
    /// Localisation is registered here rather than in the host: that the UI reads
    /// its text from resources is a property of this layer, while which culture
    /// the process runs in is the host's decision. MudBlazor is registered here
    /// for the same reason — the host knows nothing about the component library.
    /// </remarks>
    public static IServiceCollection AddCashPrismWeb(this IServiceCollection services)
    {
        services.AddLocalization();

        services.AddMudServices();

        // MudBlazor asks this for its own strings and falls back to its English
        // defaults wherever the resource file stays silent.
        services.AddScoped<MudLocalizer, ResourceMudLocalizer>();

        // Per circuit, not per page: an import outlives the page it was started
        // from, so what is running has to be remembered somewhere that survives
        // navigating away and back.
        services.AddScoped<ImportActivity>();

        // Per circuit as well: whoever shows what is stored listens here, and
        // each browser connection hears only about its own changes.
        services.AddScoped<StoredDataChanges>();

        // Scoped like the readers it is built on. The drawer resolves it in a
        // scope of its own, so it never shares a database context with a page.
        services.AddScoped<LibrarySummaryReader>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(AccessCookie.Configure);

        // Every endpoint needs a login unless it says otherwise. The exceptions
        // are declared where they are mapped, each with its reason: the login
        // and setup pages and the static assets.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }
}
