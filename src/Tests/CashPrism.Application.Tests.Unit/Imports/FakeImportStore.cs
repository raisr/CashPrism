using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;

namespace CashPrism.Application.Tests.Unit.Imports;

/// <summary>
/// An in-memory stand-in for the real store, keeping the two things the use case
/// relies on: a booking is stored once per fingerprint, and a raw row once per run
/// and fingerprint. It also records how often it was asked to write, which is what
/// the batching tests read.
/// </summary>
internal sealed class FakeImportStore : IImportStore
{
    private readonly Dictionary<string, Booking> bookings = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, ImportRun> runs = [];
    private readonly List<RawRow> rawRows = [];

    internal IReadOnlyDictionary<string, Booking> Bookings => bookings;

    internal IReadOnlyList<RawRow> RawRows => rawRows;

    internal IReadOnlyDictionary<Guid, ImportRun> Runs => runs;

    /// <summary>How many batches were written, one entry per call with its size.</summary>
    internal List<int> AppliedBatchSizes { get; } = [];

    /// <summary>How many fingerprints each state lookup asked for, one entry per call.</summary>
    internal List<int> LoadedBatchSizes { get; } = [];

    internal bool WasCompleted { get; private set; }

    public Task<ImportRun?> FindRunByFileHashAsync(
        string fileHash,
        CancellationToken cancellationToken = default)
        => Task.FromResult(runs.Values
            .FirstOrDefault(run => string.Equals(run.FileHash, fileHash, StringComparison.Ordinal)));

    public Task AddRunAsync(ImportRun run, CancellationToken cancellationToken = default)
    {
        runs[run.Id] = run;

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<string, StoredBookingState>> LoadStatesAsync(
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default)
    {
        LoadedBatchSizes.Add(fingerprints.Count);

        var states = new Dictionary<string, StoredBookingState>(StringComparer.Ordinal);

        foreach (var fingerprint in fingerprints)
        {
            if (!bookings.TryGetValue(fingerprint, out var booking))
            {
                continue;
            }

            // The run that wrote the booking wrote its raw row too, which is what
            // makes the stored state findable by one key rather than by a scan.
            var rawRow = rawRows.Single(row =>
                row.ImportRunId == booking.SourceImportRunId
                && string.Equals(row.Fingerprint, fingerprint, StringComparison.Ordinal));

            states[fingerprint] = new StoredBookingState(
                fingerprint,
                runs[booking.SourceImportRunId],
                rawRow.Json);
        }

        return Task.FromResult<IReadOnlyDictionary<string, StoredBookingState>>(states);
    }

    public Task ApplyAsync(ImportBatch batch, CancellationToken cancellationToken = default)
    {
        AppliedBatchSizes.Add(batch.BookingsToInsert.Count + batch.BookingsToUpdate.Count);

        foreach (var booking in batch.BookingsToInsert)
        {
            bookings.Add(booking.Fingerprint, booking);
        }

        foreach (var booking in batch.BookingsToUpdate)
        {
            bookings[booking.Fingerprint] = booking;
        }

        rawRows.AddRange(batch.RawRowsToStore);

        return Task.CompletedTask;
    }

    public Task CompleteRunAsync(ImportRun run, CancellationToken cancellationToken = default)
    {
        WasCompleted = true;
        runs[run.Id] = run;

        return Task.CompletedTask;
    }
}
