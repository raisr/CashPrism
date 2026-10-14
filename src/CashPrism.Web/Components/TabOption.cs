namespace CashPrism.Web.Components;

/// <summary>One tab of a <c>Tabs</c> row.</summary>
/// <typeparam name="TValue">What choosing it sets.</typeparam>
/// <param name="Value">What choosing it sets.</param>
/// <param name="Label">What the tab says, already localised.</param>
/// <param name="Count">A count shown beside the label, already formatted; none when <see langword="null"/>.</param>
public sealed record TabOption<TValue>(TValue Value, string Label, string? Count = null);
