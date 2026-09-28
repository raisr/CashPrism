using System.Net;
using CashPrism.Application.Imports;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.TestSupport.Xlsx;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Imports;

/// <summary>
/// The list of past imports as the host serves it. The ordering and paging live
/// in <c>ImportRunReader</c> and the formatting in <c>ImportRunFormat</c>, both
/// tested where they are; what only a rendered page can show is that the route
/// resolves, that the container hands the page its reader, and which of the two
/// faces — the empty state or the table — a database puts in front of a person.
/// </summary>
/// <remarks>
/// The rows themselves are not asserted here. <c>MudDataGrid</c> fetches its
/// server data after the first interactive render, so a prerendered response
/// carries the frame and not the runs.
/// </remarks>
public sealed class ImportRunsPageTests
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
    /// like a FinanzGuru export.
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
    /// The page's HTML, with entities resolved: Blazor writes every non-ASCII
    /// character numerically, so a German string would never match otherwise.
    /// </summary>
    private static async Task<string> GetAsync(CashPrismWebApplicationFactory factory)
    {
        using var client = factory.CreateClient();

        return WebUtility.HtmlDecode(await client.GetStringAsync("/imports"));
    }

    public sealed class TheListOfPastImports
    {
        [Fact]
        public async Task Answers_With_Ok()
        {
            using var factory = StartHost();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/imports");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// The page injects <c>IImportRunReader</c>. If the composition root
        /// stopped registering it, prerendering this route would throw rather
        /// than return a page.
        /// </summary>
        [Fact]
        public async Task Resolves_Its_Reader_From_The_Container()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Unhandled exception", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Importverlauf", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Takes_Its_Text_From_The_Resources_Not_From_The_Markup()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("ImportRunsEmpty", html, StringComparison.Ordinal);
            Assert.DoesNotContain("ImportRunsColumn", html, StringComparison.Ordinal);
        }

        /// <summary>
        /// The drawer is rendered from <c>NavigationItems</c>, so a destination
        /// that is missing there is a page nobody can reach.
        /// </summary>
        [Fact]
        public async Task Is_Reachable_From_The_Navigation()
        {
            using var factory = StartHost();
            using var client = factory.CreateClient();

            var html = WebUtility.HtmlDecode(await client.GetStringAsync("/"));

            Assert.Contains("href=\"/imports\"", html, StringComparison.Ordinal);
            Assert.Contains("Importverlauf", html, StringComparison.Ordinal);
        }
    }

    public sealed class WithNothingImported
    {
        [Fact]
        public async Task Says_That_Nothing_Has_Been_Imported_Yet()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.Contains("Es wurde noch nichts importiert.", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Points_At_The_Upload_Page()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.Contains("href=\"/import\"", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Shows_No_Table_At_All()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Exportdatum", html, StringComparison.Ordinal);
        }
    }

    public sealed class WithAnImportRecorded
    {
        [Fact]
        public async Task Shows_The_Columns_The_List_Is_Made_Of()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("Zeitpunkt", html, StringComparison.Ordinal);
            Assert.Contains("Datei", html, StringComparison.Ordinal);
            Assert.Contains("Exportdatum", html, StringComparison.Ordinal);
            Assert.Contains("Prüfsumme", html, StringComparison.Ordinal);
            Assert.Contains("Zeilen", html, StringComparison.Ordinal);
            Assert.Contains("Neu", html, StringComparison.Ordinal);
            Assert.Contains("Aktualisiert", html, StringComparison.Ordinal);
            Assert.Contains("Unverändert", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Drops_The_Empty_State()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Es wurde noch nichts importiert.", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Offers_The_Pager_In_German()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("Zeilen pro Seite:", html, StringComparison.Ordinal);
        }
    }
}
