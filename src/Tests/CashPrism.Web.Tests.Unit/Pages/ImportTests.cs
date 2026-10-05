using Bunit;
using CashPrism.Application.Imports;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Imports;
using CashPrism.Web.Localisation;
using CashPrism.Web.StoredData;
using CashPrism.Web.Tests.Unit.Imports;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Import = CashPrism.Web.Pages.Import;

namespace CashPrism.Web.Tests.Unit.Pages;

public sealed class ImportTests
{
    // Noon, so the local day is the same on every machine that runs this.
    private static readonly DateTimeOffset Noon = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    public abstract class PageTest : IAsyncLifetime
    {
        protected PageTest()
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddScoped<StoredDataChanges>();
            Context.Services.AddScoped<ImportActivity>();
            Context.Services.AddSingleton<IImportStore>(new FakeImportStore());
            Context.Services.AddSingleton<IClock>(new FixedClock(DateTimeOffset.UnixEpoch));
            Context.Services.AddSingleton<IImportRunReader>(Runs);

            // Only for the page to resolve its use case; a test that uploads
            // registers the source it needs, and the later registration wins.
            Context.Services.AddSingleton<IImportSource>(FakeImportSource.Failing(ImportError.NoWorksheet()));
            Context.Services.AddScoped<Importer>();
        }

        protected BunitContext Context { get; } = new();

        protected FakeImportRunReader Runs { get; } = new();

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await Context.DisposeAsync();

        /// <summary>
        /// Renders the page with the real use case behind it, reading from
        /// <paramref name="source"/>, and picks a file.
        /// </summary>
        protected IRenderedComponent<Import> Upload(IImportSource source)
        {
            Context.Services.AddSingleton(source);

            var page = Context.Render<Import>();

            page.FindComponent<InputFile>().UploadFiles(
                InputFileContent.CreateFromBinary([1, 2, 3], "export.xlsx"));

            return page;
        }
    }

    public sealed class OnFilePickedAsync : PageTest
    {
        [Fact]
        public void Names_The_Worksheet_Whose_Name_Carries_No_Export_Date()
        {
            var page = Upload(FakeImportSource.Failing(ImportError.SheetNameWithoutExportDate("Tabelle1", "_Export_Alle_Buchungen")));

            page.WaitForAssertion(() => Assert.Contains(
                "Das Tabellenblatt heißt „Tabelle1“.",
                page.Markup,
                StringComparison.Ordinal));
        }

        [Fact]
        public void Names_A_Missing_Column()
        {
            var page = Upload(FakeImportSource.Failing(ImportError.MissingColumns(["Tags"])));

            page.WaitForAssertion(() => Assert.Contains(
                "Im Export fehlen diese Spalten: Tags",
                page.Markup,
                StringComparison.Ordinal));
        }

        [Fact]
        public void Names_The_Column_And_The_Row_Of_A_Bad_Value()
        {
            var page = Upload(FakeImportSource.Failing(ImportError.NotAnAmount("Betrag", 4, "zwölf")));

            page.WaitForAssertion(() => Assert.Contains(
                "Spalte „Betrag“ in Zeile 4 enthält „zwölf“ statt eines Betrags.",
                page.Markup,
                StringComparison.Ordinal));
        }

        [Fact]
        public void Shows_A_Refused_File_As_A_Danger_Alert()
        {
            var page = Upload(FakeImportSource.Failing(ImportError.NoWorksheet()));

            page.WaitForAssertion(() => Assert.Equal(
                "Das ist keine Finanzguru-Datei",
                page.Find(".cp-alert--danger .cp-alert__title").TextContent));
        }

        /// <summary>
        /// The history reads again once an import has finished, so the run
        /// just made turns up without a reload.
        /// </summary>
        [Fact]
        public void Reads_The_History_Again_Once_The_Import_Has_Finished()
        {
            var page = Upload(FakeImportSource.Failing(ImportError.NoWorksheet()));
            Runs.Add(Noon, fileName: "Finanzguru_Alle_Buchungen_20261001.xlsx");

            Context.Services.GetRequiredService<ImportActivity>().Report(
                new ImportFeedbackMessage(ImportFeedbackSeverity.Info, "erneut", Details: []));

            page.WaitForAssertion(() => Assert.Equal(
                "Finanzguru_Alle_Buchungen_20261001.xlsx",
                page.Find(".cp-import-history__file").TextContent));
        }
    }

    public sealed class History : PageTest
    {
        [Fact]
        public void Is_Left_Out_Before_The_First_Import()
        {
            var page = Context.Render<Import>();

            Assert.Empty(page.FindAll(".cp-import-history"));
        }

        [Fact]
        public void Lists_A_Run_With_Its_Figures()
        {
            Runs.Add(Noon, fileName: "Finanzguru_Alle_Buchungen_20261001.xlsx", rowsRead: 1047, inserted: 46, updated: 3);

            var page = Context.Render<Import>();

            var cells = page.FindAll(".cp-import-history tbody td");
            Assert.Equal("Finanzguru_Alle_Buchungen_20261001.xlsx", page.Find(".cp-import-history__file").TextContent);
            Assert.Equal($"+{ImportRunFormat.Count(46)}", cells[2].TextContent.Trim());
            Assert.Equal(ImportRunFormat.Count(3), cells[3].TextContent.Trim());
            Assert.Equal(ImportRunFormat.Count(1047), cells[4].TextContent.Trim());
        }

        [Fact]
        public void Shows_A_Run_Without_New_Bookings_As_A_Plain_Zero()
        {
            Runs.Add(Noon, rowsRead: 1047, inserted: 0, updated: 3);

            var page = Context.Render<Import>();

            var cells = page.FindAll(".cp-import-history tbody td");
            Assert.Empty(page.FindAll(".cp-import-history .cp-badge--success"));
            Assert.Equal("0", cells[2].TextContent.Trim());
        }

        [Fact]
        public void Keeps_The_Checksum_On_The_File_Name()
        {
            var run = Runs.Add(Noon);

            var page = Context.Render<Import>();

            Assert.Equal(run.FileHash, page.Find(".cp-import-history__file").GetAttribute("title"));
        }

        [Fact]
        public void Says_When_The_Export_Was_Taken()
        {
            Runs.Add(Noon, exportedOn: new DateOnly(2026, 10, 1));

            var page = Context.Render<Import>();

            Assert.Equal(
                $"Exportiert am {ImportRunFormat.ExportedOn(new DateOnly(2026, 10, 1))}",
                page.Find(".cp-import-history .cp-party__reference").TextContent);
        }

        [Fact]
        public void Says_So_When_The_Export_Date_Is_Unknown()
        {
            Runs.Add(Noon, exportedOn: null);

            var page = Context.Render<Import>();

            Assert.Equal("Exportiert – Datum unbekannt", page.Find(".cp-import-history .cp-party__reference").TextContent);
        }

        [Fact]
        public void Is_Not_Read_While_An_Import_Runs()
        {
            Runs.Add(Noon);
            var release = new TaskCompletionSource();
            var activity = Context.Services.GetRequiredService<ImportActivity>();
            _ = activity.RunAsync(async () =>
            {
                await release.Task;

                return new ImportFeedbackMessage(ImportFeedbackSeverity.Info, "fertig", Details: []);
            });

            var page = Context.Render<Import>();

            Assert.Empty(page.FindAll(".cp-import-history"));
            Assert.NotNull(page.Find(".cp-dropzone--busy"));

            release.SetResult();
        }

        /// <summary>
        /// The overlay dims the whole page while an import runs; the card with
        /// the drop zone is lifted above it, so the spinner is not dimmed with
        /// everything else.
        /// </summary>
        [Fact]
        public void Lifts_The_Drop_Zone_Above_The_Overlay_Only_While_An_Import_Runs()
        {
            var release = new TaskCompletionSource();
            var activity = Context.Services.GetRequiredService<ImportActivity>();
            var running = activity.RunAsync(async () =>
            {
                await release.Task;

                return new ImportFeedbackMessage(ImportFeedbackSeverity.Info, "fertig", Details: []);
            });
            var page = Context.Render<Import>();

            Assert.NotNull(page.Find(".cp-import-busy .cp-dropzone--busy"));

            release.SetResult();
            page.WaitForAssertion(() => Assert.Empty(page.FindAll(".cp-import-busy")));
            Assert.True(running.IsCompleted);
        }

        /// <summary>
        /// The upload is read from the file input after the import has
        /// started. Taking the input out of the page while it runs left the
        /// read waiting for ever in a real browser — a test with the file in
        /// memory never noticed.
        /// </summary>
        [Fact]
        public void Keeps_The_File_Input_In_The_Page_While_An_Import_Runs()
        {
            var release = new TaskCompletionSource();
            var activity = Context.Services.GetRequiredService<ImportActivity>();
            _ = activity.RunAsync(async () =>
            {
                await release.Task;

                return new ImportFeedbackMessage(ImportFeedbackSeverity.Info, "fertig", Details: []);
            });

            var page = Context.Render<Import>();

            Assert.True(page.Find(".cp-dropzone--busy input[type=file]").HasAttribute("disabled"));

            release.SetResult();
        }
    }
}
