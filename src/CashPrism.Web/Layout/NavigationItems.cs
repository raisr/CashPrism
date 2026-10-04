namespace CashPrism.Web.Layout;

/// <summary>
/// One destination in the navigation drawer.
/// </summary>
/// <param name="Href">The route, without a leading slash for anything but the start page.</param>
/// <param name="TitleKey">The key in <c>Strings.resx</c> holding the German label.</param>
/// <param name="Icon">The Lucide name of the destination's glyph, as <c>CpIcon</c> takes it.</param>
/// <param name="SectionKey">
/// The key of the heading the destination is grouped under, or <c>null</c> for
/// the destinations at the top that belong to no group.
/// </param>
public sealed record NavigationItem(string Href, string TitleKey, string Icon, string? SectionKey = null);

/// <summary>
/// The application's destinations, in the order the drawer lists them. One list
/// serves both the drawer and the title in the app bar, so a route cannot end up
/// labelled one way in the menu and another in the header.
/// </summary>
public static class NavigationItems
{
    /// <summary>
    /// The booking list — named on its own because the drawer shows how many
    /// bookings are stored beside it.
    /// </summary>
    public static NavigationItem Bookings { get; } = new("/bookings", "NavBookings", "receipt-text");

    /// <summary>
    /// Every destination the navigation reaches. Destinations of one section
    /// stand next to each other: the drawer writes a heading wherever the
    /// section changes.
    /// </summary>
    public static IReadOnlyList<NavigationItem> All { get; } =
    [
        new("/", "NavOverview", "layout-dashboard"),
        Bookings,
        new("/import", "NavImport", "upload", "NavSectionData"),
        new("/imports", "NavImportRuns", "history", "NavSectionData"),
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
