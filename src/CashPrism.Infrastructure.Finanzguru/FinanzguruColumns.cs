namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The columns a FinanzGuru export is known to carry, in the order the export
/// writes them. The order is documentation only — nothing resolves a column by
/// its position, see <see cref="FinanzguruColumnMap"/> for why.
/// </summary>
/// <remarks>
/// The format itself is described in <c>docs/finanzguru-export.md</c>.
/// </remarks>
public static class FinanzguruColumns
{
    /// <summary>The date the booking was posted. Column A in the export.</summary>
    public const string BookingDate = "Buchungstag";

    /// <summary>The IBAN or provider handle of the account the booking belongs to.</summary>
    public const string AccountReference = "Referenzkonto";

    /// <summary>The display name the account carries in FinanzGuru.</summary>
    public const string AccountName = "Name Referenzkonto";

    /// <summary>The signed booking amount.</summary>
    public const string Amount = "Betrag";

    /// <summary>The account balance the export reports for the booking.</summary>
    public const string Balance = "Kontostand";

    /// <summary>The ISO 4217 currency code of the booking.</summary>
    public const string Currency = "Waehrung";

    /// <summary>The other party of the booking.</summary>
    public const string Counterparty = "Beguenstigter/Auftraggeber";

    /// <summary>The other party's account, which is not always an IBAN.</summary>
    public const string CounterpartyIban = "IBAN Beguenstigter/Auftraggeber";

    /// <summary>The payment reference the bank transmitted.</summary>
    public const string PaymentReference = "Verwendungszweck";

    /// <summary>The SEPA end-to-end reference.</summary>
    public const string EndToEndReference = "E-Ref";

    /// <summary>The SEPA mandate reference of a direct debit.</summary>
    public const string MandateReference = "Mandatsreferenz";

    /// <summary>The creditor identifier of a direct debit.</summary>
    public const string CreditorId = "Glaeubiger-ID";

    /// <summary>The top-level category FinanzGuru assigned.</summary>
    public const string MainCategory = "Analyse-Hauptkategorie";

    /// <summary>The sub-category FinanzGuru assigned.</summary>
    public const string SubCategory = "Analyse-Unterkategorie";

    /// <summary>Whether FinanzGuru recognised the booking as belonging to a contract.</summary>
    public const string IsContract = "Analyse-Vertrag";

    /// <summary>How often the recognised contract recurs.</summary>
    public const string ContractInterval = "Analyse-Vertragsturnus";

    /// <summary>FinanzGuru's identifier of the recognised contract.</summary>
    public const string ContractId = "Analyse-Vertrags-ID";

    /// <summary>Whether the booking is a transfer between two of your own accounts.</summary>
    public const string IsInternalTransfer = "Analyse-Umbuchung";

    /// <summary>Whether the booking is excluded from the freely disposable income.</summary>
    public const string ExcludedFromDisposableIncome = "Analyse-Vom frei verfuegbaren Einkommen ausgeschlossen";

    /// <summary>How the booking was paid, as classified by FinanzGuru.</summary>
    public const string TransactionKind = "Analyse-Umsatzart";

    /// <summary>Whether the booking counts as income or as spending.</summary>
    public const string AmountDirection = "Analyse-Betrag";

    /// <summary>The calendar week the booking falls into.</summary>
    public const string Week = "Analyse-Woche";

    /// <summary>The month the booking falls into.</summary>
    public const string Month = "Analyse-Monat";

    /// <summary>The quarter the booking falls into.</summary>
    public const string Quarter = "Analyse-Quartal";

    /// <summary>The year the booking falls into.</summary>
    public const string Year = "Analyse-Jahr";

    /// <summary>FinanzGuru's identifier of the booking.</summary>
    public const string BookingId = "Buchungs-ID";

    /// <summary>The booking a split part points back to.</summary>
    public const string OriginalReferenceId = "Referenz-Original-ID";

    /// <summary>The role a row plays in a split booking.</summary>
    public const string SplitType = "Split-Typ";

    /// <summary>The free-text tags a person put on the booking.</summary>
    public const string Tags = "Tags";

    /// <summary>
    /// Every known column, in export order. A header row has to carry all of
    /// them for <see cref="FinanzguruColumnMap.Resolve"/> to succeed.
    /// </summary>
    public static readonly IReadOnlyList<string> All =
    [
        BookingDate,
        AccountReference,
        AccountName,
        Amount,
        Balance,
        Currency,
        Counterparty,
        CounterpartyIban,
        PaymentReference,
        EndToEndReference,
        MandateReference,
        CreditorId,
        MainCategory,
        SubCategory,
        IsContract,
        ContractInterval,
        ContractId,
        IsInternalTransfer,
        ExcludedFromDisposableIncome,
        TransactionKind,
        AmountDirection,
        Week,
        Month,
        Quarter,
        Year,
        BookingId,
        OriginalReferenceId,
        SplitType,
        Tags,
    ];
}
