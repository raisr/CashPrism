using CashPrism.Application.Bookings;
using CashPrism.Application.Imports;
using CashPrism.Application.Paging;

namespace CashPrism.Web.Layout;

/// <summary>
/// Reads the <see cref="LibrarySummary"/> the drawer shows. Both figures in one
/// place, because they go stale at the same moment — when an import completes —
/// and are read again together.
/// </summary>
public sealed class LibrarySummaryReader
{
    // Newest first is the order the reader hands runs out in, so the first of a
    // one-item page is the latest.
    private static readonly PageRequest Latest = new(0, 1);

    private readonly IBookingReader bookings;
    private readonly IImportRunReader importRuns;

    /// <summary>Creates the reader over the stored bookings and import runs.</summary>
    /// <param name="bookings">Counts the bookings.</param>
    /// <param name="importRuns">Finds the latest import run.</param>
    public LibrarySummaryReader(IBookingReader bookings, IImportRunReader importRuns)
    {
        ArgumentNullException.ThrowIfNull(bookings);
        ArgumentNullException.ThrowIfNull(importRuns);

        this.bookings = bookings;
        this.importRuns = importRuns;
    }

    /// <summary>
    /// Reads both figures. One after the other rather than at once: they share
    /// a database context, and a context takes one query at a time.
    /// </summary>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<LibrarySummary> ReadAsync(CancellationToken cancellationToken = default)
    {
        var count = await bookings.CountAsync(cancellationToken);
        var latest = await importRuns.ReadPageAsync(Latest, cancellationToken);

        return new LibrarySummary(count, latest.Items.FirstOrDefault()?.ImportedAt);
    }
}
