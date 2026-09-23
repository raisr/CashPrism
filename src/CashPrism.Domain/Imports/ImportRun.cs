namespace CashPrism.Domain.Imports;

/// <summary>
/// One processed export file. It records what was imported and when, so that a
/// re-import can tell which of two states of the same booking is the later one.
/// </summary>
/// <remarks>
/// The file itself is not kept — about 1.2 MB per export that buys nothing once
/// the rows are stored. The hash is, so the same file can be recognised.
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
