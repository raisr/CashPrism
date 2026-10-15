using CashPrism.Web.Components;

namespace CashPrism.Web.Overview;

/// <summary>
/// Which way a tile's arrow points. It follows the figure the tile shows
/// beside it, rounded the way <see cref="OverviewFormat"/> rounds it, so an
/// arrow never points up next to <c>0 %</c>.
/// </summary>
public static class OverviewTrend
{
    /// <summary>The trend of a change given as a fraction, shown in whole per cent.</summary>
    /// <param name="change">The change, or <see langword="null"/> when there is nothing to compare with.</param>
    public static StatTrend OfChange(decimal? change)
        => change is { } value ? Of(Math.Round(value * 100m, MidpointRounding.AwayFromZero)) : StatTrend.Unknown;

    /// <summary>The trend of a difference in cents, shown in whole euros.</summary>
    /// <param name="differenceInCents">The difference in whole cents.</param>
    public static StatTrend OfDifference(long differenceInCents)
        => Of(Math.Round(differenceInCents / 100m, MidpointRounding.AwayFromZero));

    private static StatTrend Of(decimal shown) => shown switch
    {
        > 0 => StatTrend.Up,
        < 0 => StatTrend.Down,
        _ => StatTrend.Flat,
    };
}
