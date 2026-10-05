using CashPrism.Domain.Bookings;

namespace CashPrism.TestSupport.Bookings;

/// <summary>
/// Builds a <see cref="Booking"/> with plain values for everything a test does
/// not care about, so a test names only the values its assertion rests on.
/// </summary>
/// <remarks>
/// Shared because the booking's constructor takes every stored column: without
/// this, each test project spelled all of them out again, and every new column
/// meant the same edit in each copy.
/// </remarks>
public static class TestBookings
{
    /// <summary>The fingerprint a booking gets unless the test names one.</summary>
    public const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";

    /// <summary>The import run a booking comes from unless the test names one.</summary>
    public static readonly Guid AnImportRunId = Guid.Parse("8f3b1c2d-4e5f-4a6b-8c9d-0e1f2a3b4c5d");

    /// <summary>The booking date a booking gets unless the test names one.</summary>
    public static readonly DateTime ABookingDate = new(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// Creates a booking. Every parameter mirrors the constructor's and has a
    /// plain default; a test passes only the ones it is about.
    /// </summary>
    public static Booking Create(
        string fingerprint = AFingerprint,
        DateTime? bookedOn = null,
        long amountInCents = -6317,
        string currency = "EUR",
        string accountReference = "DE02120300000000202051",
        string accountName = "Girokonto",
        string counterparty = "Supermarkt",
        string counterpartyAccount = "",
        string paymentReference = "",
        string category = "Lebensmittel",
        string subCategory = "Supermarkt",
        bool isTransfer = false,
        SplitRole splitRole = SplitRole.None,
        string? originalFingerprint = null,
        Guid? sourceImportRunId = null)
        => new(
            fingerprint,
            bookedOn ?? ABookingDate,
            amountInCents,
            currency,
            accountReference,
            accountName,
            counterparty,
            counterpartyAccount,
            paymentReference,
            category,
            subCategory,
            isTransfer,
            splitRole,
            originalFingerprint,
            sourceImportRunId ?? AnImportRunId);
}
