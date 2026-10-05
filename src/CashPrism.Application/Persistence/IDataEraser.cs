namespace CashPrism.Application.Persistence;

/// <summary>
/// Deletes everything an import has stored, so a fresh import rebuilds it.
/// </summary>
/// <remarks>
/// Every Finanzguru export carries the complete history, so nothing is lost
/// that the next import does not bring back. The schema stays: only the data
/// goes.
/// </remarks>
public interface IDataEraser
{
    /// <summary>
    /// Deletes every booking, raw row and import run, all or nothing, and then
    /// gives the space they took back to the disk.
    /// </summary>
    /// <param name="cancellationToken">Cancels the deletion before it is committed.</param>
    Task EraseAllAsync(CancellationToken cancellationToken = default);
}
