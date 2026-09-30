using System.Security.Cryptography;
using CashPrism.Application.Time;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;

namespace CashPrism.Application.Imports;

/// <summary>
/// Imports a file of bookings: additive for the import runs and the raw rows,
/// projective for the bookings.
/// </summary>
/// <remarks>
/// <para>
/// The rules it applies are measurements of real exports, written down in
/// <c>docs/finanzguru-export.md</c>. In short: a booking is identified by its
/// fingerprint, a known booking is replaced only by a later export, and a raw row
/// is stored only where something actually changed.
/// </para>
/// <para>
/// It works in batches rather than a file at a time. An export carries the
/// owner's complete history — always, because that is the only export FinanzGuru
/// offers — so the number of rows grows without bound while the number that has
/// to be in memory at once does not have to.
/// </para>
/// </remarks>
public sealed class Importer
{
    /// <summary>
    /// How many bookings are decided and written together.
    /// </summary>
    /// <remarks>
    /// Small enough that the change tracker of whatever implements
    /// <see cref="IImportStore"/> stays short — an ORM's tracker degrades sharply
    /// once it holds tens of thousands of entities — and large enough that the
    /// lookup of stored states is one query per thousand rows rather than per row.
    /// </remarks>
    public const int BatchSize = 1_000;

    private readonly IImportSource source;
    private readonly IImportStore store;
    private readonly IClock clock;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="source">Reads the file into bookings.</param>
    /// <param name="store">Holds what is already known, and takes what is new.</param>
    /// <param name="clock">Says when this run happened.</param>
    public Importer(IImportSource source, IImportStore store, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);

        this.source = source;
        this.store = store;
        this.clock = clock;
    }

    /// <summary>
    /// Imports <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">
    /// The file's content. Has to be seekable: the hash is taken over all of it
    /// before it is read as a spreadsheet.
    /// </param>
    /// <param name="fileName">The name to record for the person reading the list of runs.</param>
    /// <param name="cancellationToken">Cancels the import.</param>
    /// <returns>What the import did, including the reasons it did nothing.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fileName"/> is missing, or <paramref name="stream"/> cannot seek.
    /// </exception>
    public async Task<ImportResult> ImportAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!stream.CanSeek)
        {
            throw new ArgumentException(
                "The import hashes the file before reading it, so the stream has to be seekable.",
                nameof(stream));
        }

        var fileHash = await HashAsync(stream, cancellationToken).ConfigureAwait(false);

        var earlierRun = await store
            .FindRunByFileHashAsync(fileHash, cancellationToken)
            .ConfigureAwait(false);

        if (earlierRun is not null)
        {
            return ImportResult.AlreadyImported(earlierRun.Id);
        }

        var runId = Guid.NewGuid();
        var read = await source.ReadAsync(stream, runId, cancellationToken).ConfigureAwait(false);

        if (!read.IsSuccess)
        {
            return ImportResult.Failed(read.Errors);
        }

        var data = read.Data!;
        var repeated = FindRepeatedFingerprint(data.Bookings);

        if (repeated is not null)
        {
            return ImportResult.Failed([repeated]);
        }

        var run = new ImportRun(
            runId,
            fileName,
            fileHash,
            data.SheetName,
            data.ExportedOn,
            clock.UtcNow);

        await store.AddRunAsync(run, cancellationToken).ConfigureAwait(false);

        var counts = await ApplyAsync(run, data.Bookings, cancellationToken).ConfigureAwait(false);

        run.Complete(data.Bookings.Count, counts.Inserted, counts.Updated, counts.Unchanged);

        await store.CompleteRunAsync(run, cancellationToken).ConfigureAwait(false);

        return ImportResult.Imported(
            runId,
            data.Bookings.Count,
            counts.Inserted,
            counts.Updated,
            counts.Unchanged,
            data.UnknownColumns);
    }

    private static async Task<string> HashAsync(Stream stream, CancellationToken cancellationToken)
    {
        stream.Position = 0;
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;

        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// The first repeated fingerprint in the file, or <c>null</c> when
    /// every row carries its own. It was distinct in every one of 6,324 measured
    /// rows, so a repeat means the assumption the whole identity rests on has
    /// broken — worth a failure that says so rather than a write that fails
    /// halfway through.
    /// </summary>
    private static ImportError? FindRepeatedFingerprint(IReadOnlyList<ImportedBooking> bookings)
    {
        var rowByFingerprint = new Dictionary<string, int>(bookings.Count, StringComparer.Ordinal);

        foreach (var imported in bookings)
        {
            var fingerprint = imported.Booking.Fingerprint;

            if (rowByFingerprint.TryGetValue(fingerprint, out var firstRow))
            {
                return ImportError.RepeatedBookingId(imported.RowNumber, firstRow);
            }

            rowByFingerprint[fingerprint] = imported.RowNumber;
        }

        return null;
    }

    private async Task<Counts> ApplyAsync(
        ImportRun run,
        IReadOnlyList<ImportedBooking> bookings,
        CancellationToken cancellationToken)
    {
        var counts = new Counts();

        foreach (var batch in bookings.Chunk(BatchSize))
        {
            var fingerprints = batch.Select(imported => imported.Booking.Fingerprint).ToArray();
            var stored = await store
                .LoadStatesAsync(fingerprints, cancellationToken)
                .ConfigureAwait(false);

            var toInsert = new List<Booking>();
            var toUpdate = new List<Booking>();
            var rawRows = new List<RawRow>();

            foreach (var imported in batch)
            {
                var fingerprint = imported.Booking.Fingerprint;

                if (!stored.TryGetValue(fingerprint, out var state))
                {
                    toInsert.Add(imported.Booking);
                    rawRows.Add(new RawRow(run.Id, fingerprint, imported.RawJson));
                    counts.Inserted++;

                    continue;
                }

                // Nothing changed, so there is nothing to store: the raw row would be
                // byte-identical to the one already kept.
                if (string.Equals(state.RawJson, imported.RawJson, StringComparison.Ordinal))
                {
                    counts.Unchanged++;

                    continue;
                }

                // The row differs, but this file is not the later description of the
                // booking — importing an older export must not undo a newer one. The
                // older row is not kept either: a raw row is the fallback for what the
                // stored projection leaves out, and this one describes a state that is
                // not stored.
                if (!run.IsLaterThan(state.SourceRun))
                {
                    counts.Unchanged++;

                    continue;
                }

                toUpdate.Add(imported.Booking);
                rawRows.Add(new RawRow(run.Id, fingerprint, imported.RawJson));
                counts.Updated++;
            }

            if (toInsert.Count > 0 || toUpdate.Count > 0)
            {
                await store
                    .ApplyAsync(new ImportBatch(toInsert, toUpdate, rawRows), cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return counts;
    }

    private sealed class Counts
    {
        internal int Inserted { get; set; }

        internal int Updated { get; set; }

        internal int Unchanged { get; set; }
    }
}
