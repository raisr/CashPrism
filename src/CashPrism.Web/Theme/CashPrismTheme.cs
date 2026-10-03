using MudBlazor;

namespace CashPrism.Web.Theme;

/// <summary>
/// The single <see cref="MudTheme"/> the application renders with. Held as one
/// static instance because the theme is a constant of the product, not a
/// per-circuit setting: only the light/dark switch changes at runtime, and
/// <c>MudThemeProvider</c> owns that.
/// </summary>
/// <remarks>
/// The values are the design system's (<c>design/README.md</c>, §1.3). The same
/// colours exist a second time as CSS custom properties in
/// <c>wwwroot/css/tokens.css</c>, because the design system's own component
/// classes read those rather than MudBlazor's palette.
/// </remarks>
public static class CashPrismTheme
{
    /// <summary>
    /// Manrope, bundled under <c>wwwroot/fonts</c>, then a system stack for the
    /// moment before it has loaded. MudBlazor's own default asks the browser for
    /// Roboto from <c>fonts.googleapis.com</c> — a request CashPrism must not
    /// make, because everything stays local and the machine may have no
    /// internet at all.
    /// </summary>
    private static readonly string[] FontStack =
    [
        "Manrope",
        "system-ui",
        "Segoe UI",
        "Helvetica",
        "Arial",
        "sans-serif",
    ];

    /// <summary>
    /// The one shadow a raised surface casts: barely there, and the same for
    /// every elevation a card uses. In the dark palette the border does the
    /// separating and the shadow all but disappears against the navy.
    /// </summary>
    private const string CardShadow = "0 1px 2px rgba(19,26,42,.04), 0 4px 16px -6px rgba(19,26,42,.08)";

    /// <summary>
    /// The theme, with both palettes. Which one renders is decided by
    /// <c>MudThemeProvider.IsDarkMode</c> in the layout.
    /// </summary>
    public static MudTheme Instance { get; } = Build();

    private static MudTheme Build() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#4a6cf7",
            PrimaryContrastText = "#ffffff",
            Secondary = "#5b6478",

            Background = "#f2f4f8",
            BackgroundGray = "#e9ecf2",
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#5b6478",
            DrawerIcon = "#5b6478",
            AppbarBackground = "#ffffff",
            AppbarText = "#131a2a",

            TextPrimary = "#131a2a",
            TextSecondary = "#5b6478",
            TextDisabled = "#8b93a7",

            Divider = "#e3e7ee",
            DividerLight = "#e9ecf2",
            TableLines = "#e3e7ee",
            LinesDefault = "#e3e7ee",
            LinesInputs = "#e3e7ee",
            TableHover = "rgba(19,26,42,0.04)",
            ActionDefault = "#5b6478",

            Success = "#12a150",
            Error = "#e5484d",
            Warning = "#d97f06",
            Info = "#4a6cf7",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#7b93ff",
            PrimaryContrastText = "#0a0d16",
            Secondary = "#a3abc2",

            Background = "#0a0d16",
            BackgroundGray = "#1d2337",
            Surface = "#111523",
            DrawerBackground = "#111523",
            DrawerText = "#a3abc2",
            DrawerIcon = "#a3abc2",
            AppbarBackground = "#111523",
            AppbarText = "#eef1f8",

            TextPrimary = "#eef1f8",
            TextSecondary = "#a3abc2",
            TextDisabled = "#6f7891",

            Divider = "#252c44",
            DividerLight = "#1d2337",
            TableLines = "#252c44",
            LinesDefault = "#252c44",
            LinesInputs = "#252c44",
            TableHover = "rgba(255,255,255,0.05)",
            ActionDefault = "#a3abc2",

            Success = "#3ddc84",
            Error = "#ff6b6f",
            Warning = "#f5b544",
            Info = "#7b93ff",
        },
        Typography = new Typography
        {
            // Only Default carries a font family out of the box; the other
            // entries inherit from it, so this one assignment removes Roboto
            // everywhere.
            Default = new DefaultTypography
            {
                FontFamily = FontStack,
                FontSize = "0.875rem",
                FontWeight = "500",
                LineHeight = "1.5",
            },

            // H4 is the page heading, H5 a section, H6 a card title. Headlines
            // are heavy with tight tracking; the text under them is not.
            H4 = new H4Typography
            {
                FontSize = "1.625rem",
                FontWeight = "750",
                LineHeight = "1.2",
                LetterSpacing = "-0.025em",
            },
            H5 = new H5Typography
            {
                FontSize = "1.125rem",
                FontWeight = "700",
                LineHeight = "1.3",
                LetterSpacing = "-0.015em",
            },
            H6 = new H6Typography
            {
                FontSize = "0.9375rem",
                FontWeight = "700",
                LineHeight = "1.4",
                LetterSpacing = "0",
            },
            Body1 = new Body1Typography { FontSize = "0.875rem", FontWeight = "500", LineHeight = "1.5" },
            Body2 = new Body2Typography { FontSize = "0.8125rem", FontWeight = "500", LineHeight = "1.45" },
            Caption = new CaptionTypography { FontSize = "0.75rem", FontWeight = "500", LineHeight = "1.4" },

            // The small uppercase label above a field or a table column.
            Overline = new OverlineTypography
            {
                FontSize = "0.6875rem",
                FontWeight = "700",
                LetterSpacing = "0.08em",
                TextTransform = "uppercase",
            },

            // Buttons are written in sentence case, so the theme must not
            // capitalise them.
            Button = new ButtonTypography
            {
                FontSize = "0.875rem",
                FontWeight = "700",
                TextTransform = "none",
                LetterSpacing = "-0.005em",
            },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            AppbarHeight = "68px",
            DrawerWidthLeft = "248px",
        },
        Shadows = BuildShadows(),
    };

    private static Shadow BuildShadows()
    {
        var shadows = new Shadow();
        for (var elevation = 1; elevation <= 3; elevation++)
        {
            shadows.Elevation[elevation] = CardShadow;
        }

        return shadows;
    }
}
