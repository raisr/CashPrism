using CashPrism.Application.Imports;
using CashPrism.Web.Imports;
using CashPrism.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Tests.Unit.Imports;

public sealed class ImportFeedbackTests
{
    /// <summary>
    /// The real <c>Strings.resx</c>, not a stub: half of what these assert is
    /// that a key exists and that its placeholders line up with the arguments
    /// passed to it, which a stub would answer for whether or not it did.
    /// </summary>
    private static IStringLocalizer<Strings> Text()
        => new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .BuildServiceProvider()
            .GetRequiredService<IStringLocalizer<Strings>>();

    public sealed class TooLarge
    {
        [Fact]
        public void Names_Both_The_Size_And_The_Limit()
        {
            var message = ImportFeedback.TooLarge(100L * 1024 * 1024, Text());

            Assert.Equal(ImportFeedbackSeverity.Error, message.Severity);
            Assert.Contains("100", message.Headline, StringComparison.Ordinal);
            Assert.Contains("64", message.Headline, StringComparison.Ordinal);
        }

        [Fact]
        public void Says_It_In_German_Rather_Than_Rendering_The_Key()
        {
            var message = ImportFeedback.TooLarge(ImportFeedback.MaxUploadBytes + 1, Text());

            Assert.DoesNotContain("ImportTooLarge", message.Headline, StringComparison.Ordinal);
            Assert.Contains("MB", message.Headline, StringComparison.Ordinal);
        }
    }

    public sealed class Describe
    {
        private static ImportResult Imported(
            int rowsRead = 3,
            int inserted = 1,
            int updated = 1,
            int unchanged = 1,
            IReadOnlyList<string>? unknownColumns = null)
            => ImportResult.Imported(
                Guid.NewGuid(),
                rowsRead,
                inserted,
                updated,
                unchanged,
                unknownColumns ?? []);

        [Fact]
        public void Reports_A_Successful_Import_As_Success()
            => Assert.Equal(
                ImportFeedbackSeverity.Success,
                ImportFeedback.Describe(Imported(), Text()).Severity);

        [Fact]
        public void Lists_The_Four_Counts()
        {
            var message = ImportFeedback.Describe(
                Imported(rowsRead: 6324, inserted: 12, updated: 3, unchanged: 6309),
                Text());

            Assert.Equal(4, message.Details.Count);
            Assert.Contains(message.Details, d => d.Contains("6324", StringComparison.Ordinal));
            Assert.Contains(message.Details, d => d.Contains("12", StringComparison.Ordinal));
            Assert.Contains(message.Details, d => d.Contains("6309", StringComparison.Ordinal));
        }

        /// <summary>
        /// A column we do not know means the export has changed shape. The
        /// import still stored the row, so it is a line to read rather than a
        /// failure — but it is not left out.
        /// </summary>
        [Fact]
        public void Adds_A_Line_For_A_Column_The_Export_Gained()
        {
            var message = ImportFeedback.Describe(
                Imported(unknownColumns: ["Analyse-Nebelkerze"]),
                Text());

            Assert.Equal(ImportFeedbackSeverity.Success, message.Severity);
            Assert.Equal(5, message.Details.Count);
            Assert.Contains(
                message.Details,
                d => d.Contains("Analyse-Nebelkerze", StringComparison.Ordinal));
        }

        [Fact]
        public void Reports_A_File_That_Was_Already_Imported_As_Information()
        {
            var message = ImportFeedback.Describe(ImportResult.AlreadyImported(Guid.NewGuid()), Text());

            Assert.Equal(ImportFeedbackSeverity.Info, message.Severity);
            Assert.Empty(message.Details);
        }

        /// <summary>
        /// The reasons are English sentences built where the failure was
        /// detected, and this UI is German — so they are logged rather than
        /// shown. Issue #55 is what makes them showable.
        /// </summary>
        [Fact]
        public void Refuses_A_File_Without_Putting_English_Reasons_On_The_Page()
        {
            var result = ImportResult.Failed(["Column 'Tags' is missing.", "Row 4 is not a date."]);

            var message = ImportFeedback.Describe(result, Text());

            Assert.Equal(ImportFeedbackSeverity.Error, message.Severity);
            Assert.Empty(message.Details);
        }

        /// <summary>
        /// A key the resource file does not carry comes back as the key itself,
        /// which renders as <c>ImportSucceeded</c> on the page. Pairing each
        /// headline with the key behind it is what catches that.
        /// </summary>
        [Fact]
        public void Takes_Every_Headline_From_The_Resource_File()
        {
            var headlines = new (string Key, string Headline)[]
            {
                ("ImportSucceeded", ImportFeedback.Describe(Imported(), Text()).Headline),
                ("ImportAlreadyImported",
                    ImportFeedback.Describe(ImportResult.AlreadyImported(Guid.NewGuid()), Text()).Headline),
                ("ImportFailed", ImportFeedback.Describe(ImportResult.Failed(["nope"]), Text()).Headline),
                ("ImportUnreadable", ImportFeedback.Unreadable(Text()).Headline),
            };

            Assert.All(headlines, pair => Assert.NotEqual(pair.Key, pair.Headline));
        }

        [Fact]
        public void Takes_Every_Count_Line_From_The_Resource_File()
        {
            var details = ImportFeedback.Describe(Imported(unknownColumns: ["Analyse-X"]), Text()).Details;

            Assert.All(details, detail => Assert.DoesNotContain("ImportCount", detail, StringComparison.Ordinal));
            Assert.All(details, detail => Assert.DoesNotContain("ImportUnknown", detail, StringComparison.Ordinal));
        }
    }
}
