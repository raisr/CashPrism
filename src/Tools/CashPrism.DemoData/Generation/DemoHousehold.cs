namespace CashPrism.DemoData.Generation;

/// <summary>
/// The household the demo export describes: one owner and the accounts a
/// plausible family keeps. Every name and every identifier is made up.
/// </summary>
public static class DemoHousehold
{
    /// <summary>The account owner, named on the household's own side of a transfer.</summary>
    public const string OwnerName = "Jana Beispiel";

    /// <summary>The current account salary arrives on and most contracts are paid from.</summary>
    public static readonly DemoAccount CurrentAccount =
        new(DemoIban.For("Girokonto " + OwnerName), "Girokonto", 2_400_00, "Debitk.1");

    /// <summary>The joint account everyday shopping is paid from.</summary>
    public static readonly DemoAccount JointAccount =
        new(DemoIban.For("Gemeinschaftskonto " + OwnerName), "Gemeinschaftskonto", 1_200_00, "Debitk.2");

    /// <summary>The call money account the savings go to.</summary>
    public static readonly DemoAccount CallMoneyAccount =
        new(DemoIban.For("Tagesgeld " + OwnerName), "Tagesgeld", 8_000_00);

    /// <summary>The credit card, which a real export identifies by a UUID rather than an IBAN.</summary>
    public static readonly DemoAccount CreditCard =
        new(DemoIds.Uuid("Kreditkarte " + OwnerName), "Kreditkarte", 0, "Kreditk.1");

    /// <summary>The loan account the mortgage instalments are paid into.</summary>
    public static readonly DemoAccount LoanAccount =
        new(DemoIban.For("Darlehen " + OwnerName), "Immobiliendarlehen", -186_400_00);

    /// <summary>An old savings account that is wound down early and then stays silent.</summary>
    public static readonly DemoAccount DormantAccount =
        new(DemoIban.For("Sparkonto " + OwnerName), "Altes Sparkonto", 1_850_73);

    /// <summary>The payment-provider account, which a real export identifies by a handle.</summary>
    public static readonly DemoAccount ProviderAccount = new("Paynet", "Paynet", 0);

    /// <summary>Every account of the household.</summary>
    public static readonly IReadOnlyList<DemoAccount> Accounts =
    [
        CurrentAccount,
        JointAccount,
        CallMoneyAccount,
        CreditCard,
        LoanAccount,
        DormantAccount,
        ProviderAccount,
    ];
}
