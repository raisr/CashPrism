namespace CashPrism.Web.Preferences;

/// <summary>The theme a person chose, or that they have not chosen one.</summary>
public enum ThemePreference
{
    /// <summary>Nothing chosen: the theme follows the operating system.</summary>
    System,

    /// <summary>The light theme, whatever the operating system says.</summary>
    Light,

    /// <summary>The dark theme, whatever the operating system says.</summary>
    Dark,
}
