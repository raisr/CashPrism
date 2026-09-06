using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddCashPrismWeb(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }
}
