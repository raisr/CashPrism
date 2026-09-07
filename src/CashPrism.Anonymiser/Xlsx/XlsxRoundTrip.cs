using System.IO.Compression;
using System.Text;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Reads one FinanzGuru <c>.xlsx</c> apart and puts it back together: every
/// zip entry is copied through unchanged. An <c>.xlsx</c> is a zip archive, and
/// ClosedXML would rewrite the whole workbook on the way — inline strings turn
/// into shared strings, the quirky per-row stylesheet disappears — which is
/// exactly the shape a real FinanzGuru export no longer has once that happens.
/// This ticket makes no anonymising change, so "put back together" here means
/// the output is byte-identical to the input, entry for entry; only the
/// worksheet part is read first, to check it is something this tool can
/// actually handle.
/// </summary>
public static class XlsxRoundTrip
{
    /// <summary>
    /// Validates <paramref name="inputPath"/> and writes the anonymised copy
    /// into <paramref name="outputDirectory"/> as
    /// <c>&lt;name&gt;-anonymised.xlsx</c>.
    /// </summary>
    /// <param name="force">
    /// Whether an existing output file may be overwritten. Without it, an
    /// existing file aborts the run rather than being silently replaced.
    /// </param>
    public static XlsxRoundTripResult Run(string inputPath, string outputDirectory, bool force)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, BuildOutputFileName(inputPath));

        if (File.Exists(outputPath))
        {
            if (!force)
            {
                return XlsxRoundTripResult.Failure(
                    $"{outputPath} already exists. Pass {CommandLine.AnonymiserCommandLine.ForceSwitch} to overwrite it.");
            }

            File.Delete(outputPath);
        }

        ZipArchive source;

        try
        {
            source = ZipFile.OpenRead(inputPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return XlsxRoundTripResult.Failure($"{inputPath}: {exception.Message}");
        }

        using (source)
        {
            var worksheetEntryName = WorksheetLocator.Locate(source);

            if (worksheetEntryName is null)
            {
                return XlsxRoundTripResult.Failure($"{inputPath}: could not locate the worksheet part.");
            }

            var worksheetEntry = source.GetEntry(worksheetEntryName);

            if (worksheetEntry is null)
            {
                return XlsxRoundTripResult.Failure(
                    $"{inputPath}: the worksheet part '{worksheetEntryName}' the workbook points to is missing.");
            }

            string worksheetXml;

            using (var reader = new StreamReader(worksheetEntry.Open(), Encoding.UTF8))
            {
                worksheetXml = reader.ReadToEnd();
            }

            var validation = WorksheetValidation.Validate(worksheetXml);

            if (!validation.IsSuccess)
            {
                return XlsxRoundTripResult.Failure($"{inputPath}: {validation.ErrorMessage}");
            }

            using (var destination = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                foreach (var entry in source.Entries)
                {
                    var newEntry = destination.CreateEntry(entry.FullName, CompressionLevel.Optimal);

                    using var entryStream = entry.Open();
                    using var newEntryStream = newEntry.Open();

                    entryStream.CopyTo(newEntryStream);
                }
            }

            return XlsxRoundTripResult.Success(outputPath, validation.DataRowCount, validation.DataRowCount);
        }
    }

    private static string BuildOutputFileName(string inputPath)
    {
        var name = Path.GetFileNameWithoutExtension(inputPath);
        var extension = Path.GetExtension(inputPath);

        return $"{name}-anonymised{extension}";
    }
}
