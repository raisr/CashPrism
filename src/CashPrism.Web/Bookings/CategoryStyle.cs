namespace CashPrism.Web.Bookings;

/// <summary>
/// One of the design's categories: the fixed colour and icon a booking is
/// shown with (<c>design/README.md</c> §1.5).
/// </summary>
/// <param name="NameKey">The key in <c>Strings.resx</c> holding the German name.</param>
/// <param name="Colour">A CSS colour, given as one of the design system's custom properties.</param>
/// <param name="Icon">The Lucide name of the glyph, as <c>CpIcon</c> takes it.</param>
public sealed record CategoryStyle(string NameKey, string Colour, string Icon)
{
    /// <summary>Rent, utilities, furnishing.</summary>
    public static CategoryStyle Housing { get; } = new("CategoryHousing", "var(--cp-prism-violet)", "house");

    /// <summary>Car, fuel, public transport.</summary>
    public static CategoryStyle Mobility { get; } = new("CategoryMobility", "var(--cp-prism-blue)", "car");

    /// <summary>What is paid again every month without being chosen again.</summary>
    public static CategoryStyle Subscriptions { get; } = new("CategorySubscriptions", "var(--cp-prism-cyan)", "repeat");

    /// <summary>Every kind of insurance.</summary>
    public static CategoryStyle Insurance { get; } = new("CategoryInsurance", "var(--cp-prism-teal)", "shield");

    /// <summary>Groceries.</summary>
    public static CategoryStyle Groceries { get; } = new("CategoryGroceries", "var(--cp-prism-green)", "shopping-basket");

    /// <summary>Doctors and pharmacies.</summary>
    public static CategoryStyle Health { get; } = new("CategoryHealth", "var(--cp-prism-amber)", "heart-pulse");

    /// <summary>Things bought.</summary>
    public static CategoryStyle Shopping { get; } = new("CategoryShopping", "var(--cp-prism-orange)", "shopping-bag");

    /// <summary>Spare time and eating out.</summary>
    public static CategoryStyle Leisure { get; } = new("CategoryLeisure", "var(--cp-prism-rose)", "popcorn");

    /// <summary>Everything the other categories do not claim, and every name not known here.</summary>
    public static CategoryStyle Other { get; } = new("CategoryOther", "var(--cp-prism-slate)", "circle-ellipsis");

    /// <summary>Money moved between two of the owner's own accounts.</summary>
    public static CategoryStyle Transfer { get; } = new("CategoryTransfer", "var(--cp-prism-slate)", "arrow-left-right");

    /// <summary>Money coming in.</summary>
    public static CategoryStyle Income { get; } = new("CategoryIncome", "var(--cp-income)", "banknote");

    /// <summary>
    /// Finanzguru's sub-categories that belong to a different design category
    /// than their main category. The design cuts some of Finanzguru's groups
    /// apart: groceries and eating out share one main category there, and
    /// subscriptions are spread over three.
    /// </summary>
    private static readonly Dictionary<(string Category, string SubCategory), CategoryStyle> BySubCategory = new()
    {
        [("Essen & Trinken", "Restaurants")] = Leisure,
        [("Essen & Trinken", "Lieferservice")] = Leisure,
        [("Lifestyle", "Mobilfunk")] = Subscriptions,
        [("Lifestyle", "Cloud-Dienste")] = Subscriptions,
        [("Lifestyle", "Prime-Mitgliedschaft")] = Subscriptions,
        [("Freizeit", "Serien & Filme")] = Subscriptions,
        [("Freizeit", "Musik & Podcasts")] = Subscriptions,
        [("Freizeit", "Mitgliedschaft")] = Subscriptions,
        [("Wohnen", "Internet & Telefon")] = Subscriptions,
        [("Wohnen", "Rundfunkgebuehren")] = Subscriptions,
    };

    /// <summary>
    /// Finanzguru's main categories, spelled as the measured exports spell them
    /// — the umlauts written out. A main category not listed here is
    /// <see cref="Other"/>, which is also where Finanzguru's own "Sonstiges",
    /// "Finanzen", "Kinder", "Haustiere" and "Sparen" end up.
    /// </summary>
    private static readonly Dictionary<string, CategoryStyle> ByCategory = new()
    {
        ["Einnahmen"] = Income,
        ["Wohnen"] = Housing,
        ["Mobilitaet"] = Mobility,
        ["Versicherungen"] = Insurance,
        ["Essen & Trinken"] = Groceries,
        ["Freizeit"] = Leisure,
        ["Lifestyle"] = Shopping,
        ["Drogerie"] = Shopping,
        ["Gesundheit"] = Health,
    };

    /// <summary>
    /// The design category a booking is shown in.
    /// </summary>
    /// <param name="category">Finanzguru's main category.</param>
    /// <param name="subCategory">Finanzguru's sub-category.</param>
    /// <param name="isTransfer">
    /// Whether Finanzguru marks the booking as a transfer between the owner's
    /// own accounts. It wins over any category: a transfer is neither spending
    /// nor income, whatever Finanzguru filed it under.
    /// </param>
    public static CategoryStyle For(string category, string subCategory, bool isTransfer)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(subCategory);

        if (isTransfer)
        {
            return Transfer;
        }

        if (BySubCategory.TryGetValue((category, subCategory), out var bySubCategory))
        {
            return bySubCategory;
        }

        return ByCategory.GetValueOrDefault(category, Other);
    }
}
