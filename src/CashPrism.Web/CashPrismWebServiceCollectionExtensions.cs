using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web;

/// <summary>
/// Registers the CashPrism web layer (Blazor components, auth UI, routing) with
/// the application's service container. Called by the host in <c>CashPrism.Shell</c>.
/// </summary>
public static class CashPrismWebServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required by the CashPrism web UI.
    /// </summary>
    public static IServiceCollection AddCashPrismWeb(this IServiceCollection services)
    {
        // Wiring is added as the web layer grows (Razor components, auth, ...).
        return services;
    }
}
