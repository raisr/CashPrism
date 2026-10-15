namespace CashPrism.Web.Components;

/// <summary>Which way the figure on a <c>StatTile</c> moved since the period before.</summary>
public enum StatTrend
{
    /// <summary>There is nothing to compare with.</summary>
    Unknown = 0,

    /// <summary>The figure went up.</summary>
    Up = 1,

    /// <summary>The figure went down.</summary>
    Down = 2,

    /// <summary>The figure stayed where it was.</summary>
    Flat = 3,
}
