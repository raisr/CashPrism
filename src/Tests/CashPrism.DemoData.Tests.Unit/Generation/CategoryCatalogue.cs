using CashPrism.DemoData.Generation;

namespace CashPrism.DemoData.Tests.Unit.Generation;

/// <summary>
/// The 76 pairs of main and sub-category a real export was measured to carry,
/// copied from <c>docs/finanzguru-export.md</c>. Kept apart from the generator
/// on purpose: a test that read the catalogue from the code under test could
/// not notice the code inventing a category.
/// </summary>
internal static class CategoryCatalogue
{
    private static readonly Dictionary<string, string[]> SubCategories = new(StringComparer.Ordinal)
    {
        ["Drogerie"] = ["Drogerie"],
        ["Einnahmen"] = ["Kapitalertraege", "Kindergeld", "Leistungen der Bundesagentur fuer Arbeit", "Lohn / Gehalt", "Rente/Pension", "Sonstige Einnahmen"],
        ["Essen & Trinken"] = ["Lebensmittel", "Lieferservice", "Restaurants"],
        ["Finanzen"] = ["Bankgebuehren", "Kredit", "Sonstige Finanzausgaben", "Spende", "Steuern"],
        ["Freizeit"] = ["Buecher & Zeitungen", "Gaming", "In-App-Kaeufe", "Kino", "Mitgliedschaft", "Musik & Podcasts", "Serien & Filme", "Sonstige Freizeitausgaben", "Sport", "Urlaub", "Veranstaltungen"],
        ["Gesundheit"] = ["Aerztliche Behandlung", "Apotheke", "Sonstige Gesundheitsausgaben"],
        ["Haustiere"] = ["Futter & Tierbedarf", "Tieraerztliche Behandlung"],
        ["Kinder"] = ["Kinderbetreuung", "Schule & Foerderung", "Sonstige Kinderausgaben", "Taschengeld"],
        ["Lifestyle"] = ["Bekleidung", "Bildung", "Cloud-Dienste", "Elektrohandel", "Geschenke", "Mobilfunk", "Prime-Mitgliedschaft", "Shopping", "Sonstiger Lifestyle"],
        ["Mobilitaet"] = ["Auto", "Bus & Bahn", "Fahrrad", "Sharing / Gemietet", "Tanken", "Taxi"],
        ["Sonstiges"] = ["Bargeld", "Kreditkartenabrechnung", "Sonstige Ausgaben"],
        ["Sparen"] = ["Bausparvertrag", "Sparen"],
        ["Versicherungen"] = ["Brillenversicherung", "Gesetzliche Krankenversicherung", "Haftpflichtversicherung", "Hausratversicherung", "KFZ-Versicherung", "Lebensversicherung", "Private Krankenversicherung", "Rechtsschutzversicherung", "Sonstige Sachversicherung", "Tierhaftpflichtversicherung", "Tierkrankenversicherung", "Unfallversicherung", "Wohngebaeudeversicherung"],
        ["Wohnen"] = ["Bauen / Renovieren", "Baufinanzierung", "Einrichtung", "Gas", "Internet & Telefon", "Rundfunkgebuehren", "Sonstiges Wohnen", "Strom"],
    };

    /// <summary>Every measured pair.</summary>
    public static IReadOnlySet<DemoCategory> Pairs { get; } = SubCategories
        .SelectMany(main => main.Value.Select(sub => new DemoCategory(main.Key, sub)))
        .ToHashSet();
}
