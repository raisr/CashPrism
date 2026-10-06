namespace CashPrism.DemoData.Generation;

/// <summary>
/// One generated booking, before the columns that follow from all bookings
/// together — the balance — or from the date alone — the period labels — are
/// worked out by <see cref="DemoLedger"/>.
/// </summary>
public sealed record DemoBooking
{
    /// <summary>What <c>Buchungs-ID</c> carries.</summary>
    public required string BookingId { get; init; }

    /// <summary>The household account the booking belongs to.</summary>
    public required DemoAccount Account { get; init; }

    /// <summary>The booking date; only a payment-provider payment carries a time.</summary>
    public required DateTime Date { get; init; }

    /// <summary>The signed amount: negative for spending, positive for income.</summary>
    public required long AmountInCents { get; init; }

    /// <summary>The other party of the booking.</summary>
    public required DemoParty Party { get; init; }

    /// <summary>What <c>Analyse-Hauptkategorie</c> and <c>Analyse-Unterkategorie</c> carry.</summary>
    public required DemoCategory Category { get; init; }

    /// <summary>What <c>Analyse-Umsatzart</c> carries.</summary>
    public required string TransactionKind { get; init; }

    /// <summary>What <c>Verwendungszweck</c> carries.</summary>
    public string PaymentReference { get; init; } = string.Empty;

    /// <summary>The contract the booking belongs to, or <see langword="null"/>.</summary>
    public DemoContract? Contract { get; init; }

    /// <summary>Whether the booking moves money between two of the household's accounts.</summary>
    public bool IsInternalTransfer { get; init; }

    /// <summary>Whether the booking is left out of the freely disposable income.</summary>
    public bool IsExcludedFromDisposableIncome { get; init; }

    /// <summary>What <c>Split-Typ</c> carries; empty on an ordinary booking.</summary>
    public string SplitType { get; init; } = string.Empty;

    /// <summary>The <c>Buchungs-ID</c> a split part points back to; empty otherwise.</summary>
    public string OriginalReferenceId { get; init; } = string.Empty;

    /// <summary>What <c>Tags</c> carries.</summary>
    public string Tags { get; init; } = string.Empty;
}
