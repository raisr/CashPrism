using CashPrism.Web.Access;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace CashPrism.Web.Configuration;

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
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        // Anonymous, because the login page is drawn with them: its stylesheets,
        // fonts and Blazor's own script. None of them carries anything of the
        // household's data.
        app.MapStaticAssets().AllowAnonymous();

        // A form rather than a link, so a page elsewhere cannot log someone out
        // by embedding the address; the token proves the form came from here.
        app.MapPost(AccessPaths.Logout, LogoutAsync)
            .WithMetadata(new RequireAntiforgeryTokenAttribute());

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    private static async Task LogoutAsync(HttpContext context)
    {
        // The middleware only records whether the token was valid; turning a
        // missing one away is up to the endpoint, and nothing binds a form here
        // that would do it.
        if (context.Features.Get<IAntiforgeryValidationFeature>() is not { IsValid: true })
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            return;
        }

        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        context.Response.Redirect(AccessPaths.Login);
    }
}
