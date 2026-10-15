namespace CashPrism.Web.Bookings;

/// <summary>
/// The name a person reads for how a booking was paid. Finanzguru writes the
/// kind as a German word with its umlauts spelled out (<c>Ueberweisung</c>),
/// which is right for a file and wrong on a screen.
/// </summary>
/// <remarks>
/// The seven words are the ones measured in a real export; the catalogue may be
/// larger. A word not known here has no key, and the caller shows it as it
/// stands — an unfamiliar spelling is better than a booking type that vanishes.
/// The stored value is never changed, only how it is shown.
/// </remarks>
public static class TransactionKindLabel
{
    private static readonly Dictionary<string, string> Keys = new(StringComparer.Ordinal)
    {
        ["Kartenzahlung"] = "TransactionKindCardPayment",
        ["SEPA-Lastschrift"] = "TransactionKindDirectDebit",
        ["Ueberweisung"] = "TransactionKindBankTransfer",
        ["Dauerauftrag"] = "TransactionKindStandingOrder",
        ["Barentnahme"] = "TransactionKindCashWithdrawal",
        ["Zinsen/Entgelt"] = "TransactionKindInterestAndFees",
        ["Sonstige"] = "TransactionKindOther",
    };

    /// <summary>
    /// The key in <c>Strings.resx</c> naming <paramref name="transactionKind"/>,
    /// or <c>null</c> when the word is not one of the known ones.
    /// </summary>
    /// <param name="transactionKind">The word as the export carries it.</param>
    public static string? KeyFor(string transactionKind)
        => Keys.GetValueOrDefault(transactionKind);
}
