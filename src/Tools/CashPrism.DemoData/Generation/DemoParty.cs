namespace CashPrism.DemoData.Generation;

/// <summary>
/// The other party of a booking: what <c>Beguenstigter/Auftraggeber</c>,
/// <c>IBAN Beguenstigter/Auftraggeber</c>, <c>Mandatsreferenz</c> and
/// <c>Glaeubiger-ID</c> carry. Every value is derived from the name, so the
/// same party always looks the same.
/// </summary>
/// <param name="Name">The party's name. Fictional throughout.</param>
/// <param name="Iban">The party's account — not always an IBAN, and empty for a card merchant.</param>
/// <param name="MandateReference">The mandate reference, filled only for a direct-debit creditor.</param>
/// <param name="CreditorId">The creditor identifier, filled on exactly the rows the mandate is.</param>
public sealed record DemoParty(string Name, string Iban, string MandateReference, string CreditorId)
{
    /// <summary>A party paid by transfer: a name and an IBAN.</summary>
    public static DemoParty Payee(string name) => new(name, DemoIban.For(name), string.Empty, string.Empty);

    /// <summary>A party that collects by direct debit: an IBAN, a mandate and a creditor identifier.</summary>
    public static DemoParty Creditor(string name)
        => new(
            name,
            DemoIban.For(name),
            "MR" + DemoIds.Digits(name + "|mandate", 14),
            DemoIban.CreditorId(DemoIds.Digits(name + "|creditor", 11)));

    /// <summary>A party paid by card: the terminal names the merchant and nothing else.</summary>
    public static DemoParty Merchant(string name) => new(name, string.Empty, string.Empty, string.Empty);

    /// <summary>
    /// A party paid through the payment provider, which identifies it by an email
    /// address and puts that address in the IBAN column.
    /// </summary>
    public static DemoParty ByEmail(string name, string email) => new(name, email, string.Empty, string.Empty);

    /// <summary>The household itself, as the other side of a transfer from or to <paramref name="account"/>.</summary>
    public static DemoParty Own(DemoAccount account)
        => new(DemoHousehold.OwnerName, account.Reference, string.Empty, string.Empty);
}
