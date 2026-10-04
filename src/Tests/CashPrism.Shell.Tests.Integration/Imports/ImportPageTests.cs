using System.Net;
using CashPrism.Application.Imports;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.TestSupport.Xlsx;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Imports;

/// <summary>
/// The import page as the host serves it: the drop zone and, once something
/// was imported, the list of past imports. The page's own logic lives in
/// <c>ImportFeedback</c>, <c>ImportRunReader</c> and <c>ImportRunFormat</c> and
/// is tested there; what a rendered page can show that a unit test cannot is
/// that the route resolves, the German text comes out of the resource file,
/// the container hands the page what it needs, and which face a database puts
/// in front of a person.
/// </summary>
/// <remarks>
/// The page reads its history before it renders, so the prerendered response
/// already carries the runs.
/// </remarks>
public sealed class ImportPageTests
{
    /// <summary>A host on a database of its own, already migrated.</summary>
    private static CashPrismWebApplicationFactory StartHost()
    {
        var factory = new CashPrismWebApplicationFactory();

        // Building the host is what applies the schema — see HostBootTests.
        factory.CreateClient().Dispose();

        return factory;
    }

    /// <summary>
    /// Records a run the way a person would: by importing a workbook shaped
    /// like a Finanzguru export.
    /// </summary>
    private static async Task ImportAsync(CashPrismWebApplicationFactory factory)
    {
        var workbook = XlsxTestWorkbook.Build(
            FinanzguruColumns.All,
            [FinanzguruTestRow.Create(FinanzguruTestRow.AFingerprint)],
            sheetName: "20260907_Export_Alle_Buchungen");

        await using var scope = factory.Services.CreateAsyncScope();
        using var stream = new MemoryStream(workbook);

        var result = await scope.ServiceProvider
            .GetRequiredService<Importer>()
            .ImportAsync(stream, "20260907_Export_Alle_Buchungen.xlsx");

        Assert.Equal(ImportOutcome.Imported, result.Outcome);
    }

    /// <summary>
    /// A page's HTML, with entities resolved: Blazor writes every non-ASCII
    /// character numerically, so a German string would never match otherwise.
    /// </summary>
    private static async Task<string> GetAsync(CashPrismWebApplicationFactory factory, string path = "/import")
    {
        using var client = factory.CreateClient();

        return WebUtility.HtmlDecode(await client.GetStringAsync(path));
    }

    public sealed class TheImportPage
    {
        [Fact]
        public async Task Answers_With_Ok()
        {
            using var factory = StartHost();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/import");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Offers_The_Drop_Zone()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.Contains("Finanzguru-Export hierher ziehen", html, StringComparison.Ordinal);
            Assert.Contains("accept=\".xlsx\"", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Explains_The_Steps_To_An_Export()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.Contains("So geht’s", html, StringComparison.Ordinal);
            Assert.Contains("„Alle Buchungen“ exportieren", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Takes_Its_Text_From_The_Resources_Not_From_The_Markup()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("ImportDrop", html, StringComparison.Ordinal);
            Assert.DoesNotContain("ImportStep", html, StringComparison.Ordinal);
        }

        /// <summary>
        /// The page injects <c>Importer</c> and <c>IImportRunReader</c>. If the
        /// composition root stopped registering either, prerendering this route
        /// would throw rather than return a page.
        /// </summary>
        [Fact]
        public async Task Resolves_What_It_Needs_From_The_Container()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Unhandled exception", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Finanzguru-Export hierher ziehen", html, StringComparison.Ordinal);
        }
    }

    public sealed class WithNothingImported
    {
        [Fact]
        public async Task Shows_No_History()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Bisherige Importe", html, StringComparison.Ordinal);
            Assert.DoesNotContain("<table", html, StringComparison.Ordinal);
        }
    }

    public sealed class WithAnImportRecorded
    {
        [Fact]
        public async Task Shows_The_Columns_The_History_Is_Made_Of()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("Bisherige Importe", html, StringComparison.Ordinal);
            Assert.Contains("Datei", html, StringComparison.Ordinal);
            Assert.Contains("Eingelesen", html, StringComparison.Ordinal);
            Assert.Contains("Neu", html, StringComparison.Ordinal);
            Assert.Contains("Aktualisiert", html, StringComparison.Ordinal);
            Assert.Contains("Zeilen gesamt", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Lists_The_Run_With_Its_File_And_Export_Date()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("20260907_Export_Alle_Buchungen.xlsx", html, StringComparison.Ordinal);
            Assert.Contains("Exportiert am 07.09.2026", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Counts_The_Runs_In_German()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("1–1 von 1 Importen", html, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The history used to be a page of its own. It is on the import page now,
    /// and the drawer no longer leads anywhere else.
    /// </summary>
    public sealed class TheNavigation
    {
        [Fact]
        public async Task Has_No_Separate_History_Entry()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory, "/");

            Assert.DoesNotContain("href=\"/imports\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Importverlauf", html, StringComparison.Ordinal);
        }
    }
}
