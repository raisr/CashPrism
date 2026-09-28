using CashPrism.Application.Bookings;
using CashPrism.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Reads pages of bookings out of the database.
/// </summary>
public sealed class BookingReader : IBookingReader
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the reader.
    /// </summary>
    /// <param name="context">The database.</param>
    public BookingReader(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<BookingPage> ReadPageAsync(
        BookingPageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Nothing read here is written back, and the tracker would otherwise hold
        // every booking of every page a circuit ever looked at.
        var bookings = context.Bookings.AsNoTracking();

        var total = await bookings.CountAsync(cancellationToken);

        var page = await Order(bookings, request)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new BookingPage(page, total);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return context.Bookings.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Orders by the requested column, then by the fingerprint. The second key
    /// is not decoration: hundreds of bookings share a date, and without a
    /// tiebreaker SQLite may return them in a different order for every query —
    /// which is how a pager shows the same booking twice and skips another.
    /// </summary>
    private static IQueryable<Booking> Order(IQueryable<Booking> bookings, BookingPageRequest request)
    {
        var ordered = (request.SortBy, request.Descending) switch
        {
            (BookingSortColumn.BookedOn, false) => bookings.OrderBy(b => b.BookedOn),
            (BookingSortColumn.BookedOn, true) => bookings.OrderByDescending(b => b.BookedOn),
            (BookingSortColumn.AccountName, false) => bookings.OrderBy(b => b.AccountName),
            (BookingSortColumn.AccountName, true) => bookings.OrderByDescending(b => b.AccountName),
            (BookingSortColumn.Counterparty, false) => bookings.OrderBy(b => b.Counterparty),
            (BookingSortColumn.Counterparty, true) => bookings.OrderByDescending(b => b.Counterparty),
            (BookingSortColumn.PaymentReference, false) => bookings.OrderBy(b => b.PaymentReference),
            (BookingSortColumn.PaymentReference, true) => bookings.OrderByDescending(b => b.PaymentReference),
            (BookingSortColumn.Category, false) => bookings.OrderBy(b => b.Category),
            (BookingSortColumn.Category, true) => bookings.OrderByDescending(b => b.Category),
            (BookingSortColumn.AmountInCents, false) => bookings.OrderBy(b => b.AmountInCents),
            _ => bookings.OrderByDescending(b => b.AmountInCents),
        };

        return ordered.ThenBy(b => b.Fingerprint);
    }
}
