using CashPrism.Domain.Imports;

namespace CashPrism.Application.Imports;

/// <summary>
/// The stored side of an import. Everything here is deliberately shaped around
/// one export at a time and a batch of bookings at a time, rather than around
/// single rows: FinanzGuru re-exports the complete history on every export, so a
/// per-row round trip would multiply by tens of thousands.
/// </summary>
public interface IImportStore
{
    /// <summary>
    /// The earlier run that imported the same file, or <c>null</c> when this file
    /// has not been seen. Importing a file twice is something a person may do, so
    /// it is a question rather than a constraint.
    /// </summary>
    /// <param name="fileHash">The hash over the file's bytes.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<ImportRun?> FindRunByFileHashAsync(string fileHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores <paramref name="run"/> before its rows are processed. The rows
    /// reference it, so it has to exist first — which is why its counts are filled
    /// in later by <see cref="CompleteRunAsync"/>.
    /// </summary>
    /// <param name="run">The run that is starting.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task AddRunAsync(ImportRun run, CancellationToken cancellationToken = default);

    /// <summary>
    /// What is stored about the named bookings, keyed by fingerprint. Fingerprints
    /// that are not stored yet are absent from the result rather than present with
    /// an empty value.
    /// </summary>
    /// <param name="fingerprints">The bookings to look up. One batch worth, not a whole export.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyDictionary<string, StoredBookingState>> LoadStatesAsync(
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes one batch: inserts, updates and the raw rows behind them.
    /// </summary>
    /// <param name="batch">What this batch changes.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task ApplyAsync(ImportBatch batch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the counts a completed run now carries.
    /// </summary>
    /// <param name="run">The run, already completed in memory.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task CompleteRunAsync(ImportRun run, CancellationToken cancellationToken = default);
}
