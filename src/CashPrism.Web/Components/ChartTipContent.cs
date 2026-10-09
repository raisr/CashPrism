namespace CashPrism.Web.Components;

/// <summary>What a <c>ChartTip</c> shows for one mark, and where it sits against it.</summary>
/// <param name="Title">What the mark stands for, already localised.</param>
/// <param name="Value">The figure, already formatted.</param>
/// <param name="Colour">The mark's colour.</param>
/// <param name="Anchor">Where the box sits against the mark.</param>
public sealed record ChartTipContent(string Title, string Value, string Colour, ChartTipAnchor Anchor)
{
    /// <summary>
    /// The anchor for a mark at <paramref name="position"/> of
    /// <paramref name="count"/> along the axis: leaning inwards at either end,
    /// centred everywhere else.
    /// </summary>
    public static ChartTipAnchor AnchorAt(int position, int count)
        => position == 0
            ? ChartTipAnchor.Start
            : position == count - 1 ? ChartTipAnchor.End : ChartTipAnchor.Centre;
}
