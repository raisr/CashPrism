namespace CashPrism.Web.Preferences;

/// <summary>
/// The layout choices a person makes that outlast a reload: the theme and the
/// collapsed drawer. Declared here, where the UI needs it, so a component
/// depends on the choices and not on where they are kept.
/// </summary>
public interface IPreferenceStore
{
    /// <summary>The chosen theme; <see cref="ThemePreference.System"/> until one is chosen.</summary>
    ThemePreference Theme { get; }

    /// <summary>Whether the drawer is collapsed to the rail; expanded until chosen otherwise.</summary>
    bool SidebarCollapsed { get; }

    /// <summary>Keeps <paramref name="theme"/> as the chosen theme.</summary>
    Task SetThemeAsync(ThemePreference theme, CancellationToken cancellationToken = default);

    /// <summary>Keeps whether the drawer is collapsed.</summary>
    Task SetSidebarCollapsedAsync(bool collapsed, CancellationToken cancellationToken = default);
}
