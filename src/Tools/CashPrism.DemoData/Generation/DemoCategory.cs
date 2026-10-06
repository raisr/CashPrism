namespace CashPrism.DemoData.Generation;

/// <summary>
/// A pair of <c>Analyse-Hauptkategorie</c> and <c>Analyse-Unterkategorie</c>,
/// spelled as the export spells them — umlauts written out.
/// </summary>
/// <param name="Main">The main category.</param>
/// <param name="Sub">The sub-category.</param>
public sealed record DemoCategory(string Main, string Sub);
