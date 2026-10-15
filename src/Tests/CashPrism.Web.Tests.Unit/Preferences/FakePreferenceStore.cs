using CashPrism.Web.Preferences;

namespace CashPrism.Web.Tests.Unit.Preferences;

/// <summary>Keeps the preferences in memory, starting from what a test set.</summary>
public sealed class FakePreferenceStore : IPreferenceStore
{
    /// <inheritdoc />
    public ThemePreference Theme { get; set; }

    /// <inheritdoc />
    public bool SidebarCollapsed { get; set; }

    /// <inheritdoc />
    public Task SetThemeAsync(ThemePreference theme, CancellationToken cancellationToken = default)
    {
        Theme = theme;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetSidebarCollapsedAsync(bool collapsed, CancellationToken cancellationToken = default)
    {
        SidebarCollapsed = collapsed;
        return Task.CompletedTask;
    }
}
