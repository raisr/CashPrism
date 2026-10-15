namespace CashPrism.Web.Components;

/// <summary>One choice of a <c>Segmented</c> control.</summary>
/// <typeparam name="TValue">What choosing it sets.</typeparam>
/// <param name="Value">What choosing it sets.</param>
/// <param name="Label">What the button says, already localised.</param>
public sealed record SegmentedOption<TValue>(TValue Value, string Label);
