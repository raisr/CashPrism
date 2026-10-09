namespace CashPrism.Web.Components;

/// <summary>Where a <c>ChartTip</c> sits against the mark it describes.</summary>
public enum ChartTipAnchor
{
    /// <summary>Centred above the mark.</summary>
    Centre = 0,

    /// <summary>Starting at the mark and reaching towards the end, for a mark at the chart's start.</summary>
    Start = 1,

    /// <summary>Ending at the mark, for a mark at the chart's end.</summary>
    End = 2,
}
