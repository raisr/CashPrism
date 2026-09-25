using MudBlazor;

namespace CashPrism.Web.Theme;

/// <summary>
/// The single <see cref="MudTheme"/> the application renders with. Held as one
/// static instance because the theme is a constant of the product, not a
/// per-circuit setting: only the light/dark switch changes at runtime, and
/// <c>MudThemeProvider</c> owns that.
/// </summary>
public static class CashPrismTheme
{
    /// <summary>
    /// A system font stack. MudBlazor's own default asks the browser for Roboto
    /// from <c>fonts.googleapis.com</c>, and the package ships no font file —
    /// a request CashPrism must not make, because everything stays local and the
    /// machine may have no internet at all.
    /// </summary>
    private static readonly string[] SystemFontStack =
    [
        "system-ui",
        "Segoe UI",
        "Noto Sans",
        "Helvetica",
        "Arial",
        "sans-serif",
    ];

    /// <summary>
    /// The theme, with both palettes. Which one renders is decided by
    /// <c>MudThemeProvider.IsDarkMode</c> in the layout.
    /// </summary>
    public static MudTheme Instance { get; } = Build();

    private static MudTheme Build() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3f5fd0",
            PrimaryContrastText = "#ffffff",
            Secondary = "#6b7180",

            // The content sits on a tinted ground so white cards read as raised
            // without a shadow; shadows cost contrast in dark mode.
            Background = "#f3f5fa",
            BackgroundGray = "#e9edf5",
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#16181d",
            DrawerIcon = "#6b7180",
            AppbarBackground = "#ffffff",
            AppbarText = "#16181d",

            TextPrimary = "#16181d",
            TextSecondary = "#6b7180",
            TextDisabled = "#9aa1b1",

            Divider = "#e3e5ea",
            DividerLight = "#eef0f4",
            TableLines = "#e3e5ea",
            LinesDefault = "#e3e5ea",
            LinesInputs = "#ccd1db",

            // Money: a credit is green, a debit red. Colour only reinforces the
            // sign — the minus in front of the amount is what carries it.
            Success = "#1f6b4a",
            Error = "#9c3b2e",
            Warning = "#8a5a12",
            Info = "#3f5fd0",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#97a3e8",
            PrimaryContrastText = "#101218",
            Secondary = "#9aa1b1",

            Background = "#101218",
            BackgroundGray = "#0b0d12",
            Surface = "#171a21",
            DrawerBackground = "#171a21",
            DrawerText = "#e7e9ee",
            DrawerIcon = "#9aa1b1",
            AppbarBackground = "#171a21",
            AppbarText = "#e7e9ee",

            TextPrimary = "#e7e9ee",
            TextSecondary = "#9aa1b1",
            TextDisabled = "#6b7180",

            Divider = "#262a33",
            DividerLight = "#1e222b",
            TableLines = "#262a33",
            LinesDefault = "#262a33",
            LinesInputs = "#333944",

            Success = "#6fc49b",
            Error = "#e08a7b",
            Warning = "#d8b169",
            Info = "#97a3e8",
        },
        Typography = new Typography
        {
            // Only Default carries a font family out of the box; the other
            // entries inherit from it, so this one assignment removes Roboto
            // everywhere.
            Default = new DefaultTypography { FontFamily = SystemFontStack },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "240px",
        },
    };
}
