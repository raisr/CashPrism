using CashPrism.Web.Localisation;
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

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }
}
