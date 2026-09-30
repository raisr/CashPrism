using System.Net;

namespace CashPrism.Shell.Tests.Integration.Imports;

/// <summary>
/// The upload page as the host serves it. The page's own logic lives in
/// <c>ImportFeedback</c> and is tested there; what a rendered page can show
/// that a unit test cannot is that the route resolves, the German text comes
/// out of the resource file, and the page asks for the one service it needs
/// without the container throwing.
/// </summary>
public sealed class ImportPageTests
{
    public sealed class TheUploadPage(CashPrismWebApplicationFactory factory)
        : IClassFixture<CashPrismWebApplicationFactory>
    {
        /// <summary>
        /// The page's HTML, with entities resolved: Blazor writes every
        /// non-ASCII character numerically, so <c>auswählen</c> arrives as
        /// <c>ausw&amp;#xE4;hlen</c> and a German string would never match.
        /// </summary>
        private async Task<string> GetAsync()
        {
            using var client = factory.CreateClient();

            return WebUtility.HtmlDecode(await client.GetStringAsync("/import"));
        }

        [Fact]
        public async Task Answers_With_Ok()
        {
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/import");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Offers_The_File_Picker()
        {
            var html = await GetAsync();

            Assert.Contains("Export auswählen", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Says_Which_File_It_Expects()
        {
            var html = await GetAsync();

            Assert.Contains("Finanzguru", html, StringComparison.Ordinal);
            Assert.Contains(".xlsx", html, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Takes_Its_Text_From_The_Resources_Not_From_The_Markup()
        {
            var html = await GetAsync();

            Assert.DoesNotContain("ImportPick", html, StringComparison.Ordinal);
            Assert.DoesNotContain("ImportHint", html, StringComparison.Ordinal);
        }

        /// <summary>
        /// The page injects <c>Importer</c>. If the composition root stopped
        /// registering it, prerendering this route would throw rather than
        /// return a page — which is what this asserts, without uploading
        /// anything.
        /// </summary>
        [Fact]
        public async Task Resolves_The_Import_Use_Case_From_The_Container()
        {
            var html = await GetAsync();

            Assert.DoesNotContain("Unhandled exception", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Export auswählen", html, StringComparison.Ordinal);
        }
    }
}
