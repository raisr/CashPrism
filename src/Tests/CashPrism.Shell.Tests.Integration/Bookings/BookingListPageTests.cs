using System.Net;
using CashPrism.Application.Imports;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.TestSupport.Xlsx;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Bookings;

/// <summary>
/// The booking list as the host serves it. The ordering and paging live in
/// <c>BookingReader</c> and the formatting in <c>BookingFormat</c>, both tested
/// where they are; what only a rendered page can show is that the route
/// resolves, that the container hands the page its reader, and which of the two
/// faces — the empty state or the table — a database puts in front of a person.
/// </summary>
/// <remarks>
/// The page reads its first page of bookings before it renders, so the
/// prerendered response already carries the rows. What happens on a click —
/// sorting, paging — needs a circuit and is covered by the page's bUnit tests.
/// </remarks>
public sealed class BookingListPageTests
{
    /// <summary>
    /// A host on a database of its own, already migrated. Each test gets one:
    /// what the list shows is what the test under way put there.
    /// </summary>
    private static CashPrismWebApplicationFactory StartHost()
    {
        var factory = new CashPrismWebApplicationFactory();

        // Building the host is what applies the schema — see HostBootTests.
        factory.CreateClient().Dispose();

        return factory;
    }

    /// <summary>
    /// Puts two bookings in the database the way a person would: through the
    /// real import, from a workbook shaped like a FinanzGuru export.
    /// </summary>
    private static async Task ImportTwoBookingsAsync(CashPrismWebApplicationFactory factory)
    {
        var workbook = XlsxTestWorkbook.Build(
            FinanzguruColumns.All,
            [
                FinanzguruTestRow.Create(FinanzguruTestRow.AFingerprint),
                FinanzguruTestRow.Create(FinanzguruTestRow.AnotherFingerprint, amount: "-12.00"),
            ],
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

        return WebUtility.HtmlDecode(await client.GetStringAsync("/bookings"));
    }

    public sealed class TheBookingList
    {
        [Fact]
        public async Task Answers_With_Ok()
        {
            using var factory = StartHost();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/bookings");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// The page injects <c>IBookingReader</c>. If the composition root
        /// stopped registering it, prerendering this route would throw rather
        /// than return a page.
        /// </summary>
        [Fact]
        public async Task Resolves_Its_Reader_From_The_Container()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Unhandled exception", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Buchungen", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Takes_Its_Text_From_The_Resources_Not_From_The_Markup()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.DoesNotContain("BookingsEmpty", html, StringComparison.Ordinal);
            Assert.DoesNotContain("BookingsColumn", html, StringComparison.Ordinal);
        }
    }

    public sealed class WithNothingImported
    {
        [Fact]
        public async Task Says_That_There_Are_No_Bookings_Yet()
        {
            using var factory = StartHost();

            var html = await GetAsync(factory);

            Assert.Contains("Es gibt noch keine Buchungen.", html, StringComparison.Ordinal);
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

            Assert.DoesNotContain("<table", html, StringComparison.Ordinal);
        }
    }

    public sealed class WithBookingsImported
    {
        [Fact]
        public async Task Shows_The_Columns_The_List_Is_Made_Of()
        {
            using var factory = StartHost();
            await ImportTwoBookingsAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("Empfänger / Absender", html, StringComparison.Ordinal);
            Assert.Contains("Kategorie", html, StringComparison.Ordinal);
            Assert.Contains("Konto", html, StringComparison.Ordinal);
            Assert.Contains("Datum", html, StringComparison.Ordinal);
            Assert.Contains("Betrag", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Shows_The_Imported_Bookings()
        {
            using var factory = StartHost();
            await ImportTwoBookingsAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("Supermarkt", html, StringComparison.Ordinal);
            Assert.Contains("−63,17 €", html, StringComparison.Ordinal);
            Assert.Contains("−12,00 €", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Drops_The_Empty_State()
        {
            using var factory = StartHost();
            await ImportTwoBookingsAsync(factory);

            var html = await GetAsync(factory);

            Assert.DoesNotContain("Es gibt noch keine Buchungen.", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Offers_The_Pager_In_German()
        {
            using var factory = StartHost();
            await ImportTwoBookingsAsync(factory);

            var html = await GetAsync(factory);

            Assert.Contains("Zeilen pro Seite", html, StringComparison.Ordinal);
            Assert.Contains("1–2 von 2 Buchungen", html, StringComparison.Ordinal);
        }
    }
}
