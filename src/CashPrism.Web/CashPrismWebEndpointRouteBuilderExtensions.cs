using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CashPrism.Web;

/// <summary>
/// Maps the CashPrism web endpoints (Blazor root component, auth pages, static
/// assets) onto the host's request pipeline. Called by <c>CashPrism.Shell</c>.
/// </summary>
public static class CashPrismWebEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the CashPrism web UI endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapCashPrismWeb(this IEndpointRouteBuilder endpoints)
    {
        // Endpoints are added as the web layer grows (Razor components, auth, ...).
        return endpoints;
    }
}
