namespace CashPrism.Domain.Bookings;

/// <summary>
/// A single booking, identified by its fingerprint. It is a projection of the
/// latest export that carried the booking, not an immutable record: the same
/// booking is enriched between exports, so a re-import replaces it rather than
/// adding a second one.
/// </summary>
/// <remarks>
/// <para>
/// The export carries 29 columns and this model keeps 14 of them. The four
/// period columns are left out because they reproduce exactly from the booking
/// date and therefore carry nothing; the rest stays available in the stored raw
/// row for the day it is needed. The reasoning, and the measurements behind it,
/// are in <c>docs/finanzguru-export.md</c>.
/// </para>
/// <para>
/// A booking is deliberately not a record: two bookings are the same booking
/// when their fingerprints match, whatever their other fields say — that is the
/// whole point of the fingerprint, and value equality over all 14 fields would
/// answer a different question. Use <see cref="IsSameBookingAs"/>.
/// </para>
/// </remarks>
public sealed class Booking
{
    /// <summary>
    /// Creates a booking. The guard clauses reject what cannot be a booking at
    /// all; they deliberately do not police the shape of the values, because the
    /// shapes we know are measurements of two export files rather than anything
    /// FinanzGuru guarantees.
    /// </summary>
    /// <param name="fingerprint">
    /// The FinanzGuru <c>Buchungs-ID</c>, the identity of the booking. Required.
    /// </param>
    /// <param name="bookedOn">
    /// The date the booking was posted. Keeps the time component the export
    /// stores on some rows, so that dropping it stays a decision taken where
    /// bookings are compared rather than silently here.
    /// </param>
    /// <param name="amountInCents">
    /// The signed amount in whole cents: negative for spending, positive for income.
    /// Cents rather than a fractional type because that is what money is — an amount
    /// of the smallest unit the currency has — and because the number then survives
    /// storage, sorting and totalling untouched. The unit is in the name so no reader
    /// has to guess it.
    /// </param>
    /// <param name="currency">The ISO 4217 code the amount is in. Required.</param>
    /// <param name="accountReference">
    /// The account the booking belongs to — an IBAN, or the handle a provider
    /// account is identified by. Required.
    /// </param>
    /// <param name="accountName">The display name that account carries in FinanzGuru.</param>
    /// <param name="counterparty">The other party of the booking, as the bank transmitted it.</param>
    /// <param name="counterpartyAccount">
    /// The other party's account. Not always an IBAN: it may also be a UUID, an
    /// email address or a short opaque code. Empty where the export has no value.
    /// </param>
    /// <param name="paymentReference">The payment reference. Empty where the export has none.</param>
    /// <param name="category">FinanzGuru's top-level category. Free text from our point of view.</param>
    /// <param name="subCategory">FinanzGuru's sub-category. Same caveat.</param>
    /// <param name="isTransfer">
    /// Whether the booking moves money between two of the owner's own accounts.
    /// A total that counts a transfer as income or as spending is wrong.
    /// </param>
    /// <param name="splitRole">The role the booking plays in a split booking.</param>
    /// <param name="originalFingerprint">
    /// The fingerprint of the booking this one is a part of, or <c>null</c> when
    /// it is not a part.
    /// </param>
    /// <exception cref="ArgumentException">
    /// A required value is missing, or <paramref name="splitRole"/> says the
    /// booking is a part while <paramref name="originalFingerprint"/> does not
    /// say which booking it is a part of.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="splitRole"/> is not a known role.
    /// </exception>
    public Booking(
        string fingerprint,
        DateTime bookedOn,
        long amountInCents,
        string currency,
        string accountReference,
        string accountName,
        string counterparty,
        string counterpartyAccount,
        string paymentReference,
        string category,
        string subCategory,
        bool isTransfer,
        SplitRole splitRole,
        string? originalFingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountReference);
        ArgumentNullException.ThrowIfNull(accountName);
        ArgumentNullException.ThrowIfNull(counterparty);
        ArgumentNullException.ThrowIfNull(counterpartyAccount);
        ArgumentNullException.ThrowIfNull(paymentReference);
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(subCategory);

        if (!Enum.IsDefined(splitRole))
        {
            throw new ArgumentOutOfRangeException(nameof(splitRole), splitRole, "Unknown split role.");
        }

        // A part without the booking it belongs to cannot be reconciled with its
        // original, which is the only reason the role is stored at all. The
        // opposite direction is not asserted: one split booking in 6,324 rows
        // says too little about which roles may carry a back-reference.
        if (IsPart(splitRole) && string.IsNullOrWhiteSpace(originalFingerprint))
        {
            throw new ArgumentException(
                $"A booking with split role {splitRole} needs the fingerprint of the booking it is part of.",
                nameof(originalFingerprint));
        }

        Fingerprint = fingerprint;
        BookedOn = bookedOn;
        AmountInCents = amountInCents;
        Currency = currency;
        AccountReference = accountReference;
        AccountName = accountName;
        Counterparty = counterparty;
        CounterpartyAccount = counterpartyAccount;
        PaymentReference = paymentReference;
        Category = category;
        SubCategory = subCategory;
        IsTransfer = isTransfer;
        SplitRole = splitRole;
        OriginalFingerprint = originalFingerprint;
    }

    /// <summary>The FinanzGuru <c>Buchungs-ID</c>: the identity of the booking.</summary>
    public string Fingerprint { get; }

    /// <summary>The date the booking was posted, time component included where the export carries one.</summary>
    public DateTime BookedOn { get; }

    /// <summary>The signed amount in whole cents: negative for spending, positive for income.</summary>
    public long AmountInCents { get; }

    /// <summary>The ISO 4217 code the amount is in.</summary>
    public string Currency { get; }

    /// <summary>The account the booking belongs to.</summary>
    public string AccountReference { get; }

    /// <summary>The display name that account carries in FinanzGuru.</summary>
    public string AccountName { get; }

    /// <summary>The other party of the booking.</summary>
    public string Counterparty { get; }

    /// <summary>The other party's account, which is not always an IBAN.</summary>
    public string CounterpartyAccount { get; }

    /// <summary>The payment reference.</summary>
    public string PaymentReference { get; }

    /// <summary>FinanzGuru's top-level category.</summary>
    public string Category { get; }

    /// <summary>FinanzGuru's sub-category.</summary>
    public string SubCategory { get; }

    /// <summary>Whether the booking moves money between two of the owner's own accounts.</summary>
    public bool IsTransfer { get; }

    /// <summary>The role the booking plays in a split booking.</summary>
    public SplitRole SplitRole { get; }

    /// <summary>The fingerprint of the booking this one is a part of, or <c>null</c>.</summary>
    public string? OriginalFingerprint { get; }

    /// <summary>
    /// Whether the booking is a part of a split booking, and its amount therefore
    /// already counted by the booking it points at.
    /// </summary>
    public bool IsSplitPart => IsPart(SplitRole);

    /// <summary>
    /// Whether <paramref name="other"/> is the same booking as this one. The
    /// fingerprint decides, and nothing else does: the remaining fields change
    /// from one export to the next while the booking stays the same one.
    /// </summary>
    /// <param name="other">The booking to compare with.</param>
    public bool IsSameBookingAs(Booking other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return string.Equals(Fingerprint, other.Fingerprint, StringComparison.Ordinal);
    }

    private static bool IsPart(SplitRole splitRole)
        => splitRole is SplitRole.Part or SplitRole.Remainder;
}
