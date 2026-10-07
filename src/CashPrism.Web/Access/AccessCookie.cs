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
