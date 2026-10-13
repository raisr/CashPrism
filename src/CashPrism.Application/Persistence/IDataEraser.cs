namespace CashPrism.Application.Persistence;

/// <summary>
/// Deletes everything an import has stored, so a fresh import rebuilds it.
/// </summary>
/// <remarks>
/// Every Finanzguru export carries the complete history, so nothing is lost
/// that the next import does not bring back. The schema stays: only the data
/// goes. The account groups are CashPrism's own and no export brings them
/// back, so they go only when the caller says so. The dismissed overdue
/// markers are CashPrism's own as well, but mean nothing without the bookings
/// they were dismissed against, so they always go.
/// </remarks>
public interface IDataEraser
{
    /// <summary>
    /// Deletes every booking, raw row, import run and dismissed overdue marker,
    /// all or nothing, and then gives the space they took back to the disk.
    /// </summary>
    /// <param name="includeAccountGroups">
    /// Whether the account groups and assignments go as well. Kept, a re-import
    /// puts every account back where it was; deleted, the next read starts over
    /// with a single group.
    /// </param>
    /// <param name="cancellationToken">Cancels the deletion before it is committed.</param>
    Task EraseAllAsync(bool includeAccountGroups, CancellationToken cancellationToken = default);
}
