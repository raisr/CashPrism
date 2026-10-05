using System.Net;
using CashPrism.Application.Imports;
using CashPrism.Application.Persistence;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.TestSupport.Xlsx;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Settings;

/// <summary>
/// The settings page as the host serves it, and deleting all data against the
/// real database: that the route resolves, the container hands the page the
/// eraser, and that an emptied database takes the same export again.
/// </summary>
public sealed class SettingsPageTests
{
    private const string FileName = "20260907_Export_Alle_Buchungen.xlsx";

    /// <summary>A host on a database of its own, already migrated.</summary>
    private static CashPrismWebApplicationFactory StartHost()
    {
        var factory = new CashPrismWebApplicationFactory();

        // Building the host is what applies the schema — see HostBootTests.
        factory.CreateClient().Dispose();

        return factory;
    }

    /// <summary>Imports one and the same workbook every time it is called.</summary>
    private static async Task<ImportResult> ImportAsync(CashPrismWebApplicationFactory factory)
    {
        var workbook = XlsxTestWorkbook.Build(
            FinanzguruColumns.All,
            [FinanzguruTestRow.Create(FinanzguruTestRow.AFingerprint)],
            sheetName: "20260907_Export_Alle_Buchungen");

        await using var scope = factory.Services.CreateAsyncScope();
        using var stream = new MemoryStream(workbook);

        return await scope.ServiceProvider.GetRequiredService<Importer>().ImportAsync(stream, FileName);
    }

    private static async Task EraseAsync(CashPrismWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<IDataEraser>().EraseAllAsync();
    }

    public sealed class TheSettingsPage
    {
        [Fact]
        public async Task Answers_With_Ok()
        {
            using var factory = StartHost();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/settings");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Offers_To_Delete_All_Data()
        {
            using var factory = StartHost();
            using var client = factory.CreateClient();

            var html = WebUtility.HtmlDecode(await client.GetStringAsync("/settings"));

            Assert.Contains("Alle Daten löschen", html, StringComparison.Ordinal);
        }
    }

    public sealed class DeletingAllData
    {
        [Fact]
        public async Task Lets_The_Same_Export_Be_Imported_Again()
        {
            using var factory = StartHost();
            await ImportAsync(factory);

            await EraseAsync(factory);
            var again = await ImportAsync(factory);

            Assert.Equal(ImportOutcome.Imported, again.Outcome);
        }
    }
}
