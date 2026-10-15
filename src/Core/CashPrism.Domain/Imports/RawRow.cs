namespace CashPrism.Domain.Imports;

/// <summary>
/// A row of an imported export, stored verbatim as JSON. It is the fallback for
/// everything the booking projection leaves out, and the evidence of what the
/// export actually said.
/// </summary>
/// <remarks>
/// Only rows that are new or have changed since the last import are kept. Every
/// row of every import would cost about 2 GB a year to describe roughly 7,400
/// bookings, and an unchanged row is byte-identical to the one already stored —
/// see <c>docs/finanzguru-export.md</c>.
/// </remarks>
public sealed class RawRow
{
    /// <summary>
    /// Creates a raw row.
    /// </summary>
    /// <param name="importRunId">The run that read this row.</param>
    /// <param name="fingerprint">
    /// The fingerprint of the booking the row describes. Together with the run it
    /// says which state of which booking this is.
    /// </param>
    /// <param name="json">
    /// The row as a JSON object of the export's columns, exactly as it was read.
    /// </param>
    /// <exception cref="ArgumentException">A required value is missing.</exception>
    public RawRow(Guid importRunId, string fingerprint, string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        if (importRunId == Guid.Empty)
        {
            throw new ArgumentException("A raw row needs the run that read it.", nameof(importRunId));
        }

        ImportRunId = importRunId;
        Fingerprint = fingerprint;
        Json = json;
    }

    /// <summary>The run that read this row.</summary>
    public Guid ImportRunId { get; }

    /// <summary>The fingerprint of the booking the row describes.</summary>
    public string Fingerprint { get; }

    /// <summary>The row as a JSON object of the export's columns.</summary>
    public string Json { get; }

    /// <summary>
    /// Whether <paramref name="other"/> says the same thing about the same
    /// booking. That is what decides whether a row is worth storing again.
    /// </summary>
    /// <param name="other">The row to compare with, typically the stored one.</param>
    public bool HasSameContentAs(RawRow other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return string.Equals(Fingerprint, other.Fingerprint, StringComparison.Ordinal)
            && string.Equals(Json, other.Json, StringComparison.Ordinal);
    }
}
