using System.IO.Compression;
using System.Text;
using CashPrism.Anonymiser.Anonymisation;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Opens one input file, validates its worksheet the same way
/// <see cref="XlsxAnonymiserRun"/>'s predecessor always did, and additionally
/// collects the raw values of every replaced column — the read side of the run,
/// entirely separate from writing, because every input file has to be read
/// before the shared dictionaries can be built for any of them.
/// </summary>
public static class XlsxWorksheetReader
{
    /// <summary>Reads and validates <paramref name="inputPath"/>.</summary>
    /// <param name="maxRows">
    /// The number of newest data rows to keep, or <see langword="null"/> to
    /// keep every row. Applied before <see cref="ValuesByColumn"/> is
    /// collected, so a value that only occurred in a dropped row never claims
    /// a dictionary entry.
    /// </param>
    public static XlsxWorksheetReadResult Read(string inputPath, int? maxRows)
    {
        ArgumentNullException.ThrowIfNull(inputPath);

        ZipArchive source;

        try
        {
            source = ZipFile.OpenRead(inputPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return XlsxWorksheetReadResult.Failure($"{inputPath}: {exception.Message}");
        }

        using (source)
        {
            var worksheetEntryName = WorksheetLocator.Locate(source);

            if (worksheetEntryName is null)
            {
                return XlsxWorksheetReadResult.Failure($"{inputPath}: could not locate the worksheet part.");
            }

            var worksheetEntry = source.GetEntry(worksheetEntryName);

            if (worksheetEntry is null)
            {
                return XlsxWorksheetReadResult.Failure(
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
                return XlsxWorksheetReadResult.Failure($"{inputPath}: {validation.ErrorMessage}");
            }

            var (limitedXml, retainedRowCount) = WorksheetRowLimiter.Limit(worksheetXml, maxRows, validation.DataRowCount);

            // Validation already guarantees every known column, replaced ones
            // included, resolves to exactly one header cell — so every lookup
            // below is guaranteed to find what it looks for.
            var columnLetters = InlineStringCells.ResolveColumnLetters(limitedXml);
            var replacedLetters = AnonymisationDictionaries.ReplacedColumns
                .Select(column => columnLetters[column])
                .ToHashSet(StringComparer.Ordinal);
            var valuesByLetter = InlineStringCells.CollectValues(limitedXml, replacedLetters);

            var valuesByColumn = AnonymisationDictionaries.ReplacedColumns.ToDictionary(
                column => column,
                column => (IReadOnlyList<string>)valuesByLetter[columnLetters[column]],
                StringComparer.Ordinal);

            return XlsxWorksheetReadResult.Success(
                worksheetEntryName, limitedXml, columnLetters, valuesByColumn, validation.DataRowCount, retainedRowCount);
        }
    }
}
