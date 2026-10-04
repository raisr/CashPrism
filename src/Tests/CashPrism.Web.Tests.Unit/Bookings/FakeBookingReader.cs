using CashPrism.Application.Bookings;
using CashPrism.Application.Paging;
using CashPrism.Domain.Bookings;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// Stands in for the stored bookings. It answers a page with the bookings it
/// holds, in the order they were added, and keeps every request it was asked
/// — sorting is the database's job, so what a test checks is the question the
/// page asked, not an order the fake would have to imitate.
/// </summary>
public sealed class FakeBookingReader : IBookingReader
{
    /// <summary>The bookings a page is cut from.</summary>
    public List<Booking> Stored { get; } = [];

    /// <summary>Every page request, oldest first.</summary>
    public List<BookingPageRequest> Requests { get; } = [];

    /// <summary>
    /// How many bookings the fake claims are stored when asked for the count
    /// alone. Settable on its own, so a test can import some without building
    /// them.
    /// </summary>
    public int Count { get; set; }

    public Task<Page<Booking>> ReadPageAsync(BookingPageRequest request, CancellationToken cancellationToken = default)
    {
        Requests.Add(request);

        return Task.FromResult(new Page<Booking>([.. Stored.Skip(request.Skip).Take(request.Take)], Stored.Count));
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Count);

    /// <summary>Stores a booking with the given figures and plain values for everything else.</summary>
    public Booking Add(
        DateTime bookedOn,
        long amountInCents = -4318,
        string counterparty = "Bäckerei",
        string category = "Essen & Trinken",
        string subCategory = "Lebensmittel")
    {
        var booking = new Booking(
            fingerprint: Guid.NewGuid().ToString("N"),
            bookedOn: bookedOn,
            amountInCents: amountInCents,
            currency: "EUR",
            accountReference: "DE00000000000000000000",
            accountName: "Girokonto",
            counterparty: counterparty,
            counterpartyAccount: string.Empty,
            paymentReference: "Kartenzahlung",
            category: category,
            subCategory: subCategory,
            isTransfer: false,
            splitRole: SplitRole.None,
            originalFingerprint: null,
            sourceImportRunId: Guid.NewGuid());

        Stored.Add(booking);

        return booking;
    }
}
