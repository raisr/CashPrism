using Microsoft.AspNetCore.Builder;

namespace CashPrism.Web;

/// <summary>
/// Maps the CashPrism web endpoints (Blazor root component, static assets) onto
/// the host's request pipeline. Called by <c>CashPrism.Shell</c>.
/// </summary>
public static class CashPrismWebApplicationExtensions
{
    /// <summary>
    /// Maps the CashPrism web UI endpoints and the middleware Blazor requires.
    /// Takes a <see cref="WebApplication"/> rather than an
    /// <c>IEndpointRouteBuilder</c> so the middleware order stays inside the web
    /// layer instead of leaking into the host.
    /// </summary>
    public static WebApplication MapCashPrismWeb(this WebApplication app)
    {
        app.UseAntiforgery();

        app.MapStaticAssets();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
