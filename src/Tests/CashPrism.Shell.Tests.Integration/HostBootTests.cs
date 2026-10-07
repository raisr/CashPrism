using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CashPrism.Web.Configuration;
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
        { "_content/CashPrism.Web/app.css", "font-family: \"Manrope\"" },
        { "_content/CashPrism.Web/css/tokens.css", "--cp-prism-violet" },
        { "_content/CashPrism.Web/css/components.css", ".cp-icon" },
        { "_content/CashPrism.Web/Theme/ThemeAttribute.razor.js", "export function applyTheme" },
        { "_content/CashPrism.Web/icons/lucide.css", ".icon-wallet:before" },
        { "_content/CashPrism.Web/icons/Lucide-ISC.txt", "ISC License" },
        { "_content/CashPrism.Web/fonts/Manrope-OFL.txt", "SIL OPEN FONT LICENSE" },
        { "_content/CashPrism.Web/fonts/JetBrainsMono-OFL.txt", "SIL OPEN FONT LICENSE" },
        { "_content/CashPrism.Web/favicon.svg", "<svg" },
    };

    /// <summary>
    /// The binary assets, each with the signature its format starts with. A
    /// font served as an HTML error page would still answer <c>200 OK</c>.
    /// </summary>
    public static TheoryData<string, byte[]> BinaryAssets() => new()
    {
        { "_content/CashPrism.Web/fonts/Manrope-Variable.ttf", [0x00, 0x01, 0x00, 0x00] },
        { "_content/CashPrism.Web/fonts/JetBrainsMono-Variable.ttf", [0x00, 0x01, 0x00, 0x00] },
        { "_content/CashPrism.Web/icons/lucide.woff2", "wOF2"u8.ToArray() },
        { "_content/CashPrism.Web/favicon.ico", [0x00, 0x00, 0x01, 0x00] },
    };

    /// <summary>
    /// A reference that leaves the application: an absolute URL with a scheme
    /// other than <c>data:</c>, or a protocol-relative one. The scheme is
    /// matched as text: <see cref="Uri"/> reads a rooted path such as the
    /// <c>/</c> of the base element as a <c>file:</c> URI on Linux.
    /// </summary>
    private static bool PointsElsewhere(string reference) =>
        reference.StartsWith("//", StringComparison.Ordinal)
        || (Regex.IsMatch(reference, "^[A-Za-z][A-Za-z0-9+.-]*:")
            && !reference.StartsWith("data:", StringComparison.OrdinalIgnoreCase));

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

            Assert.Contains(AppVersion.Current, html);
        }

        [Fact]
        public async Task Start_Page_Says_That_The_Data_Stays_On_This_Machine()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("Nur auf diesem Rechner", html);
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

            Assert.Contains("Deine Finanzen, lokal", html);
            Assert.DoesNotContain("BrandTagline", html);
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

        [Theory]
        [MemberData(nameof(BinaryAssets), MemberType = typeof(HostBootTests))]
        public async Task Serves_A_Binary_Asset_In_Its_Format(string path, byte[] signature)
        {
            using var client = factory.CreateClient();

            var body = await client.GetByteArrayAsync(path);

            Assert.Equal(signature, body.Take(signature.Length));
        }

        [Fact]
        public async Task Start_Page_Links_Nothing_From_Another_Host()
        {
            var html = await GetStartPageAsync();

            var references = Regex.Matches(html, """"(?:href|src)="([^"]*)"""")
                .Select(match => match.Groups[1].Value);

            Assert.DoesNotContain(references, PointsElsewhere);
        }

        [Fact]
        public async Task Stylesheets_Load_Nothing_From_Another_Host()
        {
            using var client = factory.CreateClient();
            var html = await GetStartPageAsync();
            var stylesheets = Regex.Matches(html, """"<link rel="stylesheet" href="([^"]*)"""")
                .Select(match => match.Groups[1].Value)
                .ToList();

            var references = new List<string>();
            foreach (var stylesheet in stylesheets)
            {
                var css = await client.GetStringAsync(stylesheet);
                references.AddRange(Regex.Matches(css, """(?:url\(\s*|@import\s+)["']?([^"')\s;]+)""")
                    .Select(match => match.Groups[1].Value));
            }

            Assert.NotEmpty(stylesheets);
            Assert.DoesNotContain(references, PointsElsewhere);
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

        [Theory]
        [MemberData(nameof(BinaryAssets), MemberType = typeof(HostBootTests))]
        public async Task Serves_A_Binary_Asset_In_Its_Format(string path, byte[] signature)
        {
            using var client = factory.CreateClient();

            var body = await client.GetByteArrayAsync(path);

            Assert.Equal(signature, body.Take(signature.Length));
        }
    }
}
