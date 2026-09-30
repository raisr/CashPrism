using System.Globalization;

namespace CashPrism.Web.Imports;

/// <summary>
/// How an import run's fields are written into the list of runs.
/// </summary>
public static class ImportRunFormat
{
    /// <summary>
    /// How many characters of a file hash the list shows.
    /// </summary>
    /// <remarks>
    /// A SHA-256 written as hex is 64 characters, which no column can carry
    /// beside seven others. Twelve is enough for a person to tell two runs apart
    /// and to match one against a hash they have elsewhere; the whole value goes
    /// into the cell's title, so nothing is actually hidden.
    /// </remarks>
    public const int HashPrefixLength = 12;

    /// <summary>
    /// Writes when a run happened, in the local time of the machine that hosts
    /// CashPrism — which is the machine the person ran the import on.
    /// </summary>
    /// <param name="importedAt">When the run processed the file.</param>
    /// <param name="formatProvider">
    /// The culture to format in. Defaults to the current one, which the host
    /// pins to German.
    /// </param>
    public static string ImportedAt(DateTimeOffset importedAt, IFormatProvider? formatProvider = null)
    {
        return importedAt.LocalDateTime.ToString("g", formatProvider ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Writes the date an export was taken, or <see langword="null"/> when the
    /// run has none — the sheet name did not carry one this version can read.
    /// The caller says what to put there instead, because that text is German
    /// and belongs in the resource file.
    /// </summary>
    /// <param name="exportedOn">The date the export was taken.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string? ExportedOn(DateOnly? exportedOn, IFormatProvider? formatProvider = null)
    {
        return exportedOn?.ToString("d", formatProvider ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Writes one of the counts a run reports, grouped so four and five digit
    /// figures stay readable.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string Count(int count, IFormatProvider? formatProvider = null)
    {
        return count.ToString("N0", formatProvider ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// The leading characters of a file hash — see <see cref="HashPrefixLength"/>.
    /// A hash shorter than that is written out as it is rather than padded.
    /// </summary>
    /// <param name="fileHash">The hash over the imported file's bytes.</param>
    public static string ShortHash(string fileHash)
    {
        ArgumentNullException.ThrowIfNull(fileHash);

        return fileHash.Length <= HashPrefixLength ? fileHash : fileHash[..HashPrefixLength];
    }
}
