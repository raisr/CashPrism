using System.IO.Compression;
using System.Text;
using CashPrism.Anonymiser.Anonymisation;
using CashPrism.Anonymiser.CommandLine;

namespace CashPrism.Anonymiser.Xlsx;

/// <summary>
/// Anonymises every input file of one run. Every file is read and validated
/// first, because the value dictionaries are shared across all of them and
/// have to see every file before any of them can be rewritten; only once every
/// dictionary is built does any output file get written, in the order the
/// inputs were given, stopping at the first failure.
/// </summary>
public static class XlsxAnonymiserRun
{
    /// <summary>Runs the anonymiser over <paramref name="inputPaths"/>.</summary>
    /// <param name="force">
    /// Whether an existing output file may be overwritten. Checked for every
    /// input up front, before any file is read, so a run that would fail on its
    /// last file fails before touching the first.
    /// </param>
    public static XlsxAnonymiserRunResult Run(IReadOnlyList<string> inputPaths, string outputDirectory, bool force)
    {
        ArgumentNullException.ThrowIfNull(inputPaths);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var outputPaths = inputPaths.ToDictionary(
            inputPath => inputPath, inputPath => BuildOutputPath(outputDirectory, inputPath), StringComparer.Ordinal);

        foreach (var inputPath in inputPaths)
        {
            var outputPath = outputPaths[inputPath];

            if (File.Exists(outputPath) && !force)
            {
                return XlsxAnonymiserRunResult.Failure(
                    $"{outputPath} already exists. Pass {AnonymiserCommandLine.ForceSwitch} to overwrite it.");
            }
        }

        var reads = new List<(string InputPath, XlsxWorksheetReadResult Read)>();

        foreach (var inputPath in inputPaths)
        {
            var read = XlsxWorksheetReader.Read(inputPath);

            if (!read.IsSuccess)
            {
                return XlsxAnonymiserRunResult.Failure(read.ErrorMessage!);
            }

            reads.Add((inputPath, read));
        }

        var dictionaries = AnonymisationDictionaries.Build([.. reads.Select(read => read.Read.ValuesByColumn!)]);
        var fileResults = new List<XlsxAnonymiserResult>();

        foreach (var (inputPath, read) in reads)
        {
            var outputPath = outputPaths[inputPath];

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var newWorksheetXml = WorksheetAnonymiser.Rewrite(read.WorksheetXml!, read.ColumnLetters!, dictionaries);
            var write = XlsxAnonymiserWriter.Write(inputPath, outputPath, read.WorksheetEntryName!, newWorksheetXml);

            if (!write.IsSuccess)
            {
                return XlsxAnonymiserRunResult.Failure(fileResults, write.ErrorMessage!);
            }

            var writtenWorksheetXml = ReadWorksheetBack(outputPath, read.WorksheetEntryName!);
            var leakedColumns = WorksheetSelfCheck.FindLeakedColumns(writtenWorksheetXml, read.ColumnLetters!, dictionaries);

            if (leakedColumns.Count > 0)
            {
                File.Delete(outputPath);

                return XlsxAnonymiserRunResult.Failure(
                    fileResults,
                    $"{outputPath}: self-check failed — column(s) {string.Join(", ", leakedColumns)} still carry an "
                        + "original value. The incomplete output was deleted.");
            }

            fileResults.Add(new XlsxAnonymiserResult(inputPath, outputPath, read.DataRowCount, read.DataRowCount));
        }

        return XlsxAnonymiserRunResult.Success(fileResults);
    }

    private static string ReadWorksheetBack(string outputPath, string worksheetEntryName)
    {
        using var archive = ZipFile.OpenRead(outputPath);
        using var reader = new StreamReader(archive.GetEntry(worksheetEntryName)!.Open(), Encoding.UTF8);

        return reader.ReadToEnd();
    }

    private static string BuildOutputPath(string outputDirectory, string inputPath)
    {
        var name = Path.GetFileNameWithoutExtension(inputPath);
        var extension = Path.GetExtension(inputPath);

        return Path.Combine(outputDirectory, $"{name}-anonymised{extension}");
    }
}
