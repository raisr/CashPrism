namespace CashPrism.DemoData.Generation;

/// <summary>
/// The words <c>Analyse-Umsatzart</c> carries, as a real export was measured to
/// write them.
/// </summary>
public static class DemoTransactionKind
{
    /// <summary>A payment by card.</summary>
    public const string CardPayment = "Kartenzahlung";

    /// <summary>A SEPA direct debit.</summary>
    public const string DirectDebit = "SEPA-Lastschrift";

    /// <summary>A one-off transfer.</summary>
    public const string Transfer = "Ueberweisung";

    /// <summary>A standing order.</summary>
    public const string StandingOrder = "Dauerauftrag";

    /// <summary>A cash withdrawal.</summary>
    public const string CashWithdrawal = "Barentnahme";

    /// <summary>Interest or a bank fee.</summary>
    public const string InterestAndFees = "Zinsen/Entgelt";

    /// <summary>Anything else; the payment provider's payments carry it.</summary>
    public const string Other = "Sonstige";

    /// <summary>
    /// No word at all: the export leaves the column blank on the payment
    /// provider's top-ups.
    /// </summary>
    public const string None = "";
}
