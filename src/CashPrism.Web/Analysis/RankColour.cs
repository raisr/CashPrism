namespace CashPrism.Web.Analysis;

/// <summary>
/// The colour a main category is drawn in on the analysis, chosen by its rank
/// rather than by its name. Finanzguru owns the names: a colour tied to one
/// would be lost the day it is renamed, and a name never seen before would
/// have none. A rank exists for every category, and it is fixed over the
/// longest period, so a category keeps its colour when the period changes.
/// </summary>
public static class RankColour
{
    /// <summary>
    /// The prism in the order the design lists it. The categories ranked after
    /// these share <see cref="Rest"/>.
    /// </summary>
    private static readonly string[] Colours =
    [
        "var(--cp-prism-violet)",
        "var(--cp-prism-blue)",
        "var(--cp-prism-cyan)",
        "var(--cp-prism-teal)",
        "var(--cp-prism-green)",
        "var(--cp-prism-amber)",
        "var(--cp-prism-orange)",
        "var(--cp-prism-rose)",
    ];

    /// <summary>How many categories get a colour of their own.</summary>
    public static int Distinct => Colours.Length;

    /// <summary>The colour of every category ranked after the first <see cref="Distinct"/>.</summary>
    public static string Rest => "var(--cp-prism-slate)";

    /// <summary>The colour of the category at <paramref name="rank"/>, counted from zero.</summary>
    /// <param name="rank">The category's rank in the analysis.</param>
    public static string For(int rank)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rank);

        return rank < Colours.Length ? Colours[rank] : Rest;
    }
}
