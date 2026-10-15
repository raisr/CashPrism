using CashPrism.Web.Resources;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Overview;

/// <summary>
/// The name a Finanzguru main category is shown under. Finanzguru writes the
/// umlauts out (<c>Mobilitaet</c>), and the measured names that read wrong
/// that way get their spelling from the resource file. Every other name is
/// shown as the export carries it.
/// </summary>
/// <remarks>
/// A fixed table rather than a rule: replacing every <c>ae</c> with an
/// <c>ä</c> would also rewrite a name in which the two letters belong apart.
/// The measured catalogue is in <c>docs/finanzguru-export.md</c>.
/// </remarks>
public static class MainCategoryName
{
    private static readonly Dictionary<string, string> KeysByCategory = new(StringComparer.Ordinal)
    {
        ["Mobilitaet"] = "MainCategoryMobility",
    };

    /// <summary>The name <paramref name="category"/> is shown under.</summary>
    /// <param name="category">Finanzguru's main category.</param>
    /// <param name="text">The UI's strings.</param>
    public static string For(string category, IStringLocalizer<Strings> text)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(text);

        return KeysByCategory.TryGetValue(category, out var key) ? text[key] : category;
    }
}
