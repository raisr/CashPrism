using CashPrism.Application.Bookings;
using CashPrism.Application.Paging;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.CashFlow;
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
    public async Task<Page<Booking>> ReadPageAsync(
        BookingPageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Nothing read here is written back, and the tracker would otherwise hold
        // every booking of every page a circuit ever looked at.
        var bookings = Filter(context.Bookings.AsNoTracking(), request.Filter);

        var total = await bookings.CountAsync(cancellationToken);

        var page = await Order(bookings, request)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new Page<Booking>(page, total);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return context.Bookings.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BookingSummary> ReadSummaryAsync(
        BookingFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var listed = Filter(context.Bookings.AsNoTracking(), filter);
        var summed = listed.Where(CashFlowRule.SumsInList);

        var count = await listed.CountAsync(cancellationToken);
        var income = await summed
            .Where(booking => booking.AmountInCents > 0)
            .SumAsync(booking => booking.AmountInCents, cancellationToken);
        var spending = await summed
            .Where(booking => booking.AmountInCents < 0)
            .SumAsync(booking => booking.AmountInCents, cancellationToken);

        return new BookingSummary(count, income, -spending);
    }

    /// <inheritdoc />
    public async Task<BookingFilterOptions> ReadFilterOptionsAsync(CancellationToken cancellationToken = default)
    {
        var bookings = context.Bookings.AsNoTracking();

        // One entry per reference. Should an account's name differ between
        // its bookings, the largest in ordinal order is shown, so the choice
        // is the same on every read.
        var accounts = await bookings
            .GroupBy(booking => booking.AccountReference)
            .Select(account => new BookingAccount(account.Key, account.Max(booking => booking.AccountName)!))
            .ToListAsync(cancellationToken);

        var categories = await bookings
            .Where(booking => booking.Category != string.Empty)
            .Select(booking => new { booking.Category, booking.SubCategory })
            .Distinct()
            .ToListAsync(cancellationToken);

        return new BookingFilterOptions(
            [.. accounts.OrderBy(account => account.Name, StringComparer.CurrentCulture)],
            [.. categories
                .GroupBy(pair => pair.Category, StringComparer.Ordinal)
                .OrderBy(category => category.Key, StringComparer.Ordinal)
                .Select(category => new BookingCategory(
                    category.Key,
                    [.. category
                        .Select(pair => pair.SubCategory)
                        .Where(subCategory => subCategory.Length > 0)
                        .Order(StringComparer.Ordinal)]))]);
    }

    /// <summary>
    /// Narrows the bookings to what <paramref name="filter"/> asks for. Every
    /// criterion that is set must hold.
    /// </summary>
    /// <remarks>
    /// The period's end is turned into a half-open bound on the day after it,
    /// because a booking date may carry a time and the last day would
    /// otherwise lose the bookings posted after midnight.
    /// </remarks>
    private static IQueryable<Booking> Filter(IQueryable<Booking> bookings, BookingFilter filter)
    {
        if (filter.AccountReference is { } account)
        {
            bookings = bookings.Where(booking => booking.AccountReference == account);
        }

        if (filter.Category is { } category)
        {
            bookings = bookings.Where(booking => booking.Category == category);
        }

        if (filter.SubCategory is { } subCategory)
        {
            bookings = bookings.Where(booking => booking.SubCategory == subCategory);
        }

        if (filter.Period is { } period)
        {
            var from = period.From.ToDateTime(TimeOnly.MinValue);
            var until = period.To.AddDays(1).ToDateTime(TimeOnly.MinValue);

            bookings = bookings.Where(booking => booking.BookedOn >= from && booking.BookedOn < until);
        }

        if (filter.Counterparty is { } counterparty)
        {
            bookings = bookings.Where(booking => booking.Counterparty == counterparty);
        }

        bookings = filter.Direction switch
        {
            BookingDirection.Income => bookings.Where(booking => booking.AmountInCents > 0),
            BookingDirection.Spending => bookings.Where(booking => booking.AmountInCents < 0),
            _ => bookings,
        };

        if (filter.HideTransfers)
        {
            bookings = bookings.Where(booking => !booking.IsTransfer);
        }

        return bookings;
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
