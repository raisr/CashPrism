namespace CashPrism.Web.Bookings;

/// <summary>
/// The name a person reads for how often a contract recurs. Finanzguru writes
/// the interval as a German word with its umlauts spelled out
/// (<c>vierteljaehrlich</c>), which is right for a file and wrong on a screen.
/// </summary>
/// <remarks>
/// The five words are the ones a measured export carries, the same five the
/// demo export is generated with. A word not known here has no key, and the
/// caller shows it as it stands. The stored value is never changed, only how it
/// is shown.
/// </remarks>
public static class ContractIntervalLabel
{
    private static readonly Dictionary<string, string> Keys = new(StringComparer.Ordinal)
    {
        ["monatlich"] = "ContractIntervalMonthly",
        ["zweimonatlich"] = "ContractIntervalBiMonthly",
        ["vierteljaehrlich"] = "ContractIntervalQuarterly",
        ["halbjaehrlich"] = "ContractIntervalHalfYearly",
        ["jaehrlich"] = "ContractIntervalYearly",
    };

    /// <summary>
    /// The key in <c>Strings.resx</c> naming <paramref name="contractInterval"/>,
    /// or <c>null</c> when the word is not one of the known ones.
    /// </summary>
    /// <param name="contractInterval">The word as the export carries it.</param>
    public static string? KeyFor(string contractInterval)
        => Keys.GetValueOrDefault(contractInterval);
}
