using System.IO.Compression;
using System.Text;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Writes the anonymised copy of one input file: every zip entry is copied
/// through unchanged except the worksheet part, which is replaced with the
/// already-rewritten XML — the same "take the zip apart, put it back together"
/// approach the round trip proved, now writing one part's new content instead
/// of its old one.
/// </summary>
public static class XlsxAnonymiserWriter
{
    /// <summary>
    /// Copies every entry of <paramref name="inputPath"/> into
    /// <paramref name="outputPath"/>, substituting <paramref name="worksheetXml"/>
    /// for the entry named <paramref name="worksheetEntryName"/>.
    /// </summary>
    public static XlsxAnonymiserWriteResult Write(
        string inputPath, string outputPath, string worksheetEntryName, string worksheetXml)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(worksheetEntryName);
        ArgumentNullException.ThrowIfNull(worksheetXml);

        ZipArchive source;

        try
        {
            source = ZipFile.OpenRead(inputPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return XlsxAnonymiserWriteResult.Failure($"{inputPath}: {exception.Message}");
        }

        using (source)
        {
            using var destination = ZipFile.Open(outputPath, ZipArchiveMode.Create);

            foreach (var entry in source.Entries)
            {
                var newEntry = destination.CreateEntry(entry.FullName, CompressionLevel.Optimal);

                using var newEntryStream = newEntry.Open();

                if (entry.FullName == worksheetEntryName)
                {
                    using var writer = new StreamWriter(newEntryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                    writer.Write(worksheetXml);
                }
                else
                {
                    using var entryStream = entry.Open();
                    entryStream.CopyTo(newEntryStream);
                }
            }
        }

        return XlsxAnonymiserWriteResult.Success();
    }
}
