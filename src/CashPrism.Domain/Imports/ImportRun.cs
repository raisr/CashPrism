namespace CashPrism.Domain.Imports;

/// <summary>
/// One processed export file. It records what was imported and when, so that a
/// re-import can tell which of two states of the same booking is the later one.
/// </summary>
/// <remarks>
/// <para>
/// The file itself is not kept — about 1.2 MB per export that buys nothing once
/// the rows are stored. The hash is, so the same file can be recognised.
/// </para>
/// <para>
/// The run is created before its rows are processed, because they reference it,
/// and its counts are therefore not known yet. <see cref="Complete"/> fills them
/// in once, at the end — see the remarks there.
/// </para>
/// </remarks>
public sealed class ImportRun
{
    /// <summary>
    /// Creates an import run.
    /// </summary>
    /// <param name="id">Identifies the run. Ours, not the export's.</param>
    /// <param name="fileName">The name of the imported file, for the person reading the list of runs.</param>
    /// <param name="fileHash">A hash over the file's bytes, so the same file can be recognised.</param>
    /// <param name="sheetName">
    /// The name of the worksheet the rows were read from. The export puts its
    /// date in there, which is why it is worth keeping verbatim.
    /// </param>
    /// <param name="exportedOn">
    /// The date the export was taken, read from <paramref name="sheetName"/>, or
    /// <c>null</c> when that name does not carry a date we can read.
    /// </param>
    /// <param name="importedAt">When this run processed the file.</param>
    /// <exception cref="ArgumentException">A required value is missing.</exception>
    public ImportRun(
        Guid id,
        string fileName,
        string fileHash,
        string sheetName,
        DateOnly? exportedOn,
        DateTimeOffset importedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetName);

        if (id == Guid.Empty)
        {
            throw new ArgumentException("An import run needs an identity.", nameof(id));
        }

        Id = id;
        FileName = fileName;
        FileHash = fileHash;
        SheetName = sheetName;
        ExportedOn = exportedOn;
        ImportedAt = importedAt;
    }

    /// <summary>Identifies the run.</summary>
    public Guid Id { get; }

    /// <summary>The name of the imported file.</summary>
    public string FileName { get; }

    /// <summary>A hash over the imported file's bytes.</summary>
    public string FileHash { get; }

    /// <summary>The name of the worksheet the rows were read from.</summary>
    public string SheetName { get; }

    /// <summary>The date the export was taken, or <c>null</c> when the sheet name did not carry one.</summary>
    public DateOnly? ExportedOn { get; }

    /// <summary>When this run processed the file.</summary>
    public DateTimeOffset ImportedAt { get; }

    /// <summary>How many data rows the export carried. Zero until <see cref="Complete"/> ran.</summary>
    public int RowsRead { get; private set; }

    /// <summary>How many bookings this run stored for the first time.</summary>
    public int BookingsInserted { get; private set; }

    /// <summary>How many known bookings this run replaced with a later state.</summary>
    public int BookingsUpdated { get; private set; }

    /// <summary>How many rows said nothing the stored booking did not already say.</summary>
    public int BookingsUnchanged { get; private set; }

    /// <summary>Whether <see cref="Complete"/> has run and the counts are final.</summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Records what the run did. Called once, after the last row was processed.
    /// </summary>
    /// <remarks>
    /// The counts cannot be passed to the constructor: a raw row references the run
    /// it came from, so the run has to exist before the first row is stored, and at
    /// that point nothing is counted yet. The counts sum to no asserted total on
    /// purpose — a row that is neither inserted, updated nor unchanged is a case
    /// this model does not know about yet, and an invariant here would only have to
    /// be relaxed when it turns up.
    /// </remarks>
    /// <param name="rowsRead">How many data rows the export carried.</param>
    /// <param name="bookingsInserted">How many bookings were stored for the first time.</param>
    /// <param name="bookingsUpdated">How many known bookings were replaced.</param>
    /// <param name="bookingsUnchanged">How many rows changed nothing.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is negative.</exception>
    /// <exception cref="InvalidOperationException">The run was already completed.</exception>
    public void Complete(int rowsRead, int bookingsInserted, int bookingsUpdated, int bookingsUnchanged)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rowsRead);
        ArgumentOutOfRangeException.ThrowIfNegative(bookingsInserted);
        ArgumentOutOfRangeException.ThrowIfNegative(bookingsUpdated);
        ArgumentOutOfRangeException.ThrowIfNegative(bookingsUnchanged);

        if (IsComplete)
        {
            throw new InvalidOperationException($"Import run {Id} was already completed.");
        }

        RowsRead = rowsRead;
        BookingsInserted = bookingsInserted;
        BookingsUpdated = bookingsUpdated;
        BookingsUnchanged = bookingsUnchanged;
        IsComplete = true;
    }

    /// <summary>
    /// Whether this run carries a later state of a booking than
    /// <paramref name="other"/> does, and therefore wins when both contain the
    /// same booking.
    /// </summary>
    /// <remarks>
    /// The export date decides, because that is when FinanzGuru described the
    /// booking — not when we happened to import the file. Where either run has no
    /// readable export date, the more recent import run decides instead, which is
    /// the best that is left.
    /// </remarks>
    /// <param name="other">The run to compare with.</param>
    public bool IsLaterThan(ImportRun other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (ExportedOn.HasValue && other.ExportedOn.HasValue)
        {
            return ExportedOn.Value > other.ExportedOn.Value;
        }

        return ImportedAt > other.ImportedAt;
    }
}
