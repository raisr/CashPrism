using CashPrism.Application.Imports;
using CashPrism.Web.Resources;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Imports;

/// <summary>
/// What the upload page tells the person in front of it. Separated from the
/// page so it can be read and tested without rendering anything: the page is
/// then thin enough to have nothing of its own left to assert.
/// </summary>
public static class ImportFeedback
{
    /// <summary>
    /// The largest file the page reads, in bytes.
    /// </summary>
    /// <remarks>
    /// <see cref="Microsoft.AspNetCore.Components.Forms.IBrowserFile.OpenReadStream"/>
    /// allows 512 KB unless told otherwise and throws above it, which no export
    /// has ever fit in: the measured ones are 1.15 MB. 64 MB is set instead —
    /// far above the largest export anyone is likely to have (about 28 MB for
    /// twelve years of twenty accounts, extrapolated in issue #23) and still low
    /// enough to be a limit rather than a licence.
    /// </remarks>
    public const long MaxUploadBytes = 64L * 1024 * 1024;

    /// <summary>
    /// The message for a file the page refuses to read at all.
    /// </summary>
    /// <param name="sizeInBytes">The size of the file that was offered.</param>
    /// <param name="text">The UI's strings.</param>
    public static ImportFeedbackMessage TooLarge(long sizeInBytes, IStringLocalizer<Strings> text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new ImportFeedbackMessage(
            ImportFeedbackSeverity.Error,
            text["ImportTooLarge", Megabytes(sizeInBytes), Megabytes(MaxUploadBytes)],
            Details: []);
    }

    /// <summary>
    /// The message for a file that is not a spreadsheet the reader can open,
    /// where the failure happened before the import was even reached.
    /// </summary>
    /// <param name="text">The UI's strings.</param>
    public static ImportFeedbackMessage Unreadable(IStringLocalizer<Strings> text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new ImportFeedbackMessage(ImportFeedbackSeverity.Error, text["ImportUnreadable"], Details: []);
    }

    /// <summary>
    /// What an import did, said in one headline and as many detail lines as it
    /// takes.
    /// </summary>
    /// <param name="result">The result the use case returned.</param>
    /// <param name="text">The UI's strings.</param>
    /// <exception cref="ArgumentOutOfRangeException">The result carries an outcome this does not know.</exception>
    public static ImportFeedbackMessage Describe(ImportResult result, IStringLocalizer<Strings> text)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(text);

        return result.Outcome switch
        {
            ImportOutcome.Imported => Imported(result, text),
            ImportOutcome.AlreadyImported => new ImportFeedbackMessage(
                ImportFeedbackSeverity.Info,
                text["ImportAlreadyImported"],
                Details: []),
            // The reasons are deliberately not shown. They are built where the
            // failure is detected and are therefore English sentences, and this
            // UI is German down to the last string (see Agents.md). Carrying a
            // code and its arguments across the layer instead is issue #55;
            // until then the page logs them and says only what it can say in
            // German.
            ImportOutcome.Failed => new ImportFeedbackMessage(
                ImportFeedbackSeverity.Error,
                text["ImportFailed"],
                Details: []),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.Outcome,
                "Unknown import outcome."),
        };
    }

    private static ImportFeedbackMessage Imported(ImportResult result, IStringLocalizer<Strings> text)
    {
        var details = new List<string>(5)
        {
            text["ImportCountRowsRead", result.RowsRead],
            text["ImportCountInserted", result.BookingsInserted],
            text["ImportCountUpdated", result.BookingsUpdated],
            text["ImportCountUnchanged", result.BookingsUnchanged],
        };

        // A column we do not know is not a failure — the import stored the row
        // regardless — but it is the one thing on this page worth telling
        // someone about unprompted, because it means the export has changed.
        if (result.UnknownColumns.Count > 0)
        {
            details.Add(text["ImportUnknownColumns", string.Join(", ", result.UnknownColumns)]);
        }

        return new ImportFeedbackMessage(ImportFeedbackSeverity.Success, text["ImportSucceeded"], details);
    }

    private static string Megabytes(long bytes)
        => (bytes / 1024.0 / 1024.0).ToString("0.#", System.Globalization.CultureInfo.CurrentCulture);
}
