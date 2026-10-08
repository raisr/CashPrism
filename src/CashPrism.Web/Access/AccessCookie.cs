using System.Globalization;
using System.Security.Claims;
using CashPrism.Application.Access;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Access;

/// <summary>
/// The cookie a login hands out, and where a request without one is sent.
/// </summary>
public static class AccessCookie
{
    /// <summary>
    /// How long a login lasts without being used. Sliding: every request in
    /// the second half of it starts the period again.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    /// <summary>The cookie's name, so it is recognisable among a browser's cookies.</summary>
    public const string Name = "CashPrism.Login";

    /// <summary>
    /// The claim that carries the generation of the password a login was issued
    /// under. A change or a reset raises the stored one, and every login still
    /// carrying the old value is turned away on its next request.
    /// </summary>
    public const string GenerationClaim = "cashprism:generation";

    /// <summary>Applies CashPrism's settings to the cookie scheme.</summary>
    /// <param name="options">The scheme's options.</param>
    public static void Configure(CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Cookie.Name = Name;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

        options.LoginPath = AccessPaths.Login;
        options.LogoutPath = AccessPaths.Logout;

        // A persistent cookie — "Angemeldet bleiben" — expires when the ticket
        // does. Without it the browser drops the cookie on closing, and the
        // ticket's own lifetime only matters to a browser left open.
        options.ExpireTimeSpan = Lifetime;
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = RedirectToLoginAsync;
        options.Events.OnValidatePrincipal = ValidatePrincipalAsync;
    }

    /// <summary>
    /// What a login says about the person holding it. The household shares one
    /// password, so there is nobody to name: only that whoever holds it knew
    /// the password of <paramref name="generation"/>.
    /// </summary>
    /// <param name="generation">The generation of the password that was right.</param>
    public static ClaimsPrincipal CreatePrincipal(int generation)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "CashPrism"),
                new Claim(GenerationClaim, generation.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer32),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Turns away a login issued under a password that has since been changed
    /// or reset — and one without a generation at all, issued before logins
    /// carried one. Asked on every request, so a change reaches every device
    /// the next time it asks for anything.
    /// </summary>
    private static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var validation = context.HttpContext.RequestServices.GetRequiredService<LoginValidation>();

        if (context.Principal?.FindFirst(GenerationClaim)?.Value is { } value
            && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var generation)
            && await validation.IsCurrentAsync(generation, context.HttpContext.RequestAborted))
        {
            return;
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Sends a request without a login to the login page — or, while no
    /// password is set, straight to the page that sets one, because there is
    /// nothing to log in with yet.
    /// </summary>
    private static async Task RedirectToLoginAsync(RedirectContext<CookieAuthenticationOptions> context)
    {
        var setup = context.HttpContext.RequestServices.GetRequiredService<PasswordSetup>();

        var target = await setup.IsPendingAsync(context.HttpContext.RequestAborted)
            ? AccessPaths.Setup
            : context.RedirectUri;

        context.Response.Redirect(target);
    }
}
