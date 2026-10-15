using CashPrism.Application.Paging;
using CashPrism.Domain.Imports;

namespace CashPrism.Application.Imports;

/// <summary>
/// Reads the recorded import runs for display. Separate from
/// <see cref="IImportStore"/> for the same reason the booking reader is: the
/// store writes one export at a time, this hands out one screen at a time.
/// </summary>
/// <remarks>
/// A page at a time even though the runs are few. Someone importing daily keeps
/// one run per day for as long as they use CashPrism, and a list that is only
/// short today is not a list that may be rendered whole.
/// </remarks>
public interface IImportRunReader
{
    /// <summary>
    /// Reads one page of import runs, newest first. The order is fixed: a list
    /// of what happened is read as a history, and a history has one order.
    /// </summary>
    /// <param name="request">Which slice.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<Page<ImportRun>> ReadPageAsync(PageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// How many runs are recorded. Asked before a list is drawn at all, so a
    /// database nothing was imported into can say so instead of showing an
    /// empty table.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
