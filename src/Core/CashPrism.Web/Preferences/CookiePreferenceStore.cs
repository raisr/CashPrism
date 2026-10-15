using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace CashPrism.Web.Preferences;

/// <summary>
/// Keeps the preferences in a cookie, because a cookie travels with every
/// request: the server renders the stored choice into the first response, on
/// the static pages and the prerendered ones alike, where browser storage would
/// only be read once a circuit is up.
/// </summary>
/// <remarks>
/// The cookie is read from the request while there is one — on a static page
/// and during the prerender — and carried into the circuit as persisted state,
/// since a circuit has no request to read it from. It is written from the
/// browser, because a circuit has no response to set it on either.
/// </remarks>
public sealed class CookiePreferenceStore(IHttpContextAccessor httpContextAccessor, IJSRuntime js) : IPreferenceStore
{
    /// <summary>The cookie's name.</summary>
    public const string CookieName = "CashPrism.Preferences";

    private const string ModulePath = "./_content/CashPrism.Web/js/preferences.js";
    private const int MaxAgeSeconds = 365 * 24 * 60 * 60;
    private const string ThemeKey = "theme";
    private const string SidebarKey = "sidebar";
    private const string Collapsed = "collapsed";

    private string? cookie;

    /// <summary>
    /// The cookie's value as the request carried it, or as it was last written.
    /// Public only so the prerender can hand it to the circuit.
    /// </summary>
    [PersistentState]
    public string Cookie
    {
        get => cookie ??= httpContextAccessor.HttpContext?.Request.Cookies[CookieName] ?? string.Empty;
        set => cookie = value;
    }

    /// <inheritdoc />
    public ThemePreference Theme
        => Read(ThemeKey) switch
        {
            "dark" => ThemePreference.Dark,
            "light" => ThemePreference.Light,
            _ => ThemePreference.System,
        };

    /// <inheritdoc />
    public bool SidebarCollapsed => Read(SidebarKey) == Collapsed;

    /// <inheritdoc />
    public Task SetThemeAsync(ThemePreference theme, CancellationToken cancellationToken = default)
        => WriteAsync(theme, SidebarCollapsed, cancellationToken);

    /// <inheritdoc />
    public Task SetSidebarCollapsedAsync(bool collapsed, CancellationToken cancellationToken = default)
        => WriteAsync(Theme, collapsed, cancellationToken);

    private string? Read(string key)
        => QueryHelpers.ParseQuery(Cookie).TryGetValue(key, out var value) ? value.ToString() : null;

    private async Task WriteAsync(ThemePreference theme, bool sidebarCollapsed, CancellationToken cancellationToken)
    {
        var pairs = new List<string>();
        if (theme != ThemePreference.System)
        {
            pairs.Add($"{ThemeKey}={(theme == ThemePreference.Dark ? "dark" : "light")}");
        }

        if (sidebarCollapsed)
        {
            pairs.Add($"{SidebarKey}={Collapsed}");
        }

        Cookie = string.Join('&', pairs);

        await using var module = await js.InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath);
        await module.InvokeVoidAsync("writeCookie", cancellationToken, CookieName, Cookie, MaxAgeSeconds);
    }
}
