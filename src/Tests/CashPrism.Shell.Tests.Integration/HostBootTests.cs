using System.Net;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace CashPrism.Shell.Tests.Integration;

/// <summary>
/// End-to-end smoke test for the composition root: the host must build and serve
/// the Blazor start page with the current wiring (<c>AddCashPrismWeb</c> /
/// <c>MapCashPrismWeb</c>).
/// </summary>
public sealed class HostBootTests
{
    /// <summary>
    /// Every static asset the start page loads, with something only that file
    /// carries. A static asset can answer <c>200 OK</c> with an empty body, so
    /// the marker is what tells a served file from an empty or a wrong one.
    /// </summary>
    public static TheoryData<string, string> StaticAssets() => new()
    {
        { "_framework/blazor.web.js", "Blazor-Server-Component-State:" },
        { "_content/MudBlazor/MudBlazor.min.css", ".mud-appbar{" },
        { "_content/CashPrism.Web/app.css", ".app-brand" },
    };

    public sealed class Startup(CashPrismWebApplicationFactory factory)
        : IClassFixture<CashPrismWebApplicationFactory>
    {
        private async Task<string> GetStartPageAsync()
        {
            using var client = factory.CreateClient();

            return await client.GetStringAsync("/");
        }

        [Fact]
        public async Task Answers_The_Root_Request_With_Ok()
        {
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Start_Page_Shows_The_Application_Version()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("0.1.0", html);
        }

        [Fact]
        public async Task Start_Page_Renders_The_Wiring_Marker_As_Prerendered()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("Vorgerendert", html);
        }

        [Fact]
        public async Task Start_Page_Declares_The_Language_Its_Text_Is_Written_In()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("lang=\"de\"", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Start_Page_Takes_Its_Text_From_The_Resources_Not_From_The_Markup()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("Rendermodus", html);
            Assert.DoesNotContain("Render mode", html);
        }

        [Fact]
        public async Task Start_Page_Loads_The_Blazor_Web_Script()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("_framework/blazor.web.js", html);
        }

        [Theory]
        [MemberData(nameof(StaticAssets), MemberType = typeof(HostBootTests))]
        public async Task Serves_A_Static_Asset_With_Its_Content(string path, string marker)
        {
            using var client = factory.CreateClient();

            var body = await client.GetStringAsync(path);

            Assert.Contains(marker, body, StringComparison.Ordinal);
        }

        [Fact]
        public void Creates_The_Configured_Data_Directory()
        {
            using var client = factory.CreateClient();

            Assert.True(Directory.Exists(factory.DataDirectory));
        }

        [Fact]
        public async Task Serves_The_First_Request_From_An_Already_Migrated_Database()
        {
            using var client = factory.CreateClient();

            // The request comes first on purpose: if the schema were applied later
            // than this, the assertions below would still pass while a real user hit
            // an empty database.
            using var response = await client.GetAsync("/");

            var databaseFile = Path.Combine(factory.DataDirectory, "cashprism.db");
            Assert.True(File.Exists(databaseFile), $"{databaseFile} was not created.");

            await using var connection = new SqliteConnection($"Data Source={databaseFile}");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('Bookings', 'ImportRuns', 'RawRows')";

            Assert.Equal(3L, await command.ExecuteScalarAsync());
        }
    }

    /// <summary>
    /// Production is what the executable runs as when it is started out of a
    /// build rather than from Visual Studio — and where the host, left to
    /// itself, serves every static asset empty.
    /// </summary>
    public sealed class InProduction : IDisposable
    {
        private readonly CashPrismWebApplicationFactory factory = new() { EnvironmentName = "Production" };

        public void Dispose() => factory.Dispose();

        [Theory]
        [MemberData(nameof(StaticAssets), MemberType = typeof(HostBootTests))]
        public async Task Serves_A_Static_Asset_With_Its_Content(string path, string marker)
        {
            using var client = factory.CreateClient();

            var body = await client.GetStringAsync(path);

            Assert.Contains(marker, body, StringComparison.Ordinal);
        }
    }
}
