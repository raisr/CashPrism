using MudBlazor;

namespace CashPrism.Web.Layout;

/// <summary>
/// One destination in the navigation drawer.
/// </summary>
/// <param name="Href">The route, without a leading slash for anything but the start page.</param>
/// <param name="TitleKey">The key in <c>Strings.resx</c> holding the German label.</param>
/// <param name="Icon">An SVG path from <see cref="Icons"/>.</param>
public sealed record NavigationItem(string Href, string TitleKey, string Icon);

/// <summary>
/// The application's destinations, in the order the drawer lists them. One list
/// serves both the drawer and the title in the app bar, so a route cannot end up
/// labelled one way in the menu and another in the header.
/// </summary>
public static class NavigationItems
{
    /// <summary>
    /// Every destination the navigation reaches.
    /// </summary>
    public static IReadOnlyList<NavigationItem> All { get; } =
    [
        new("/", "NavOverview", Icons.Material.Outlined.Dashboard),
        new("/bookings", "NavBookings", Icons.Material.Outlined.ReceiptLong),
        new("/import", "NavImport", Icons.Material.Outlined.UploadFile),
    ];

    /// <summary>
    /// The item a relative path belongs to, or <c>null</c> where the path is not
    /// one of the destinations. The start page matches only exactly, so it does
    /// not claim every route beneath it.
    /// </summary>
    /// <param name="relativePath">
    /// The path without the base URI and without a leading slash, as
    /// <c>NavigationManager.ToBaseRelativePath</c> returns it.
    /// </param>
    public static NavigationItem? Match(string relativePath)
    {
        var route = relativePath.Split('?')[0].Trim('/');
        var path = route.Length == 0 ? "/" : "/" + route;

        return All.FirstOrDefault(item => string.Equals(item.Href, path, StringComparison.OrdinalIgnoreCase));
    }
}
