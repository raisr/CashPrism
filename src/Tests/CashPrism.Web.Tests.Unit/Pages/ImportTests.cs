using Bunit;
using CashPrism.Application.Imports;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Imports;
using CashPrism.Web.Localisation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Import = CashPrism.Web.Pages.Import;

namespace CashPrism.Web.Tests.Unit.Pages;

public sealed class ImportTests
{
    public sealed class OnFilePickedAsync : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Names_The_Worksheet_Whose_Name_Carries_No_Export_Date()
        {
            var page = Upload(ImportError.SheetNameWithoutExportDate("Tabelle1", "_Export_Alle_Buchungen"));

            page.WaitForAssertion(() => Assert.Contains(
                "Das Tabellenblatt heißt „Tabelle1“.",
                page.Markup,
                StringComparison.Ordinal));
        }

        [Fact]
        public void Names_A_Missing_Column()
        {
            var page = Upload(ImportError.MissingColumns(["Tags"]));

            page.WaitForAssertion(() => Assert.Contains(
                "Im Export fehlen diese Spalten: Tags",
                page.Markup,
                StringComparison.Ordinal));
        }

        [Fact]
        public void Names_The_Column_And_The_Row_Of_A_Bad_Value()
        {
            var page = Upload(ImportError.NotAnAmount("Betrag", 4, "zwölf"));

            page.WaitForAssertion(() => Assert.Contains(
                "Spalte „Betrag“ in Zeile 4 enthält „zwölf“ statt eines Betrags.",
                page.Markup,
                StringComparison.Ordinal));
        }

        /// <summary>
        /// Renders the page with the real use case behind it, reading from a
        /// source that refuses the file with <paramref name="error"/>, and
        /// picks a file.
        /// </summary>
        private IRenderedComponent<Import> Upload(ImportError error)
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddMudServices();
            context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            context.Services.AddScoped<ImportActivity>();
            context.Services.AddSingleton<IImportSource>(FakeImportSource.Failing(error));
            context.Services.AddSingleton<IImportStore>(new FakeImportStore());
            context.Services.AddSingleton<IClock>(new FixedClock(DateTimeOffset.UnixEpoch));
            context.Services.AddScoped<Importer>();

            var page = context.Render<Import>();

            page.FindComponent<InputFile>().UploadFiles(
                InputFileContent.CreateFromBinary([1, 2, 3], "export.xlsx"));

            return page;
        }
    }
}
