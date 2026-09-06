using System.Net;
using System.Threading.Tasks;

namespace CashPrism.Shell.Tests.Integration;

/// <summary>
/// End-to-end smoke test for the composition root: the host must build and serve
/// the Blazor start page with the current wiring (<c>AddCashPrismWeb</c> /
/// <c>MapCashPrismWeb</c>).
/// </summary>
public sealed class HostBootTests
{
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

            Assert.Contains("Prerendered", html);
        }

        [Fact]
        public async Task Start_Page_Loads_The_Blazor_Web_Script()
        {
            var html = await GetStartPageAsync();

            Assert.Contains("_framework/blazor.web.js", html);
        }

        [Fact]
        public async Task Serves_The_Blazor_Web_Script()
        {
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("_framework/blazor.web.js");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Serves_The_Stylesheet_Of_The_Web_Library()
        {
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("_content/CashPrism.Web/app.css");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void Creates_The_Configured_Data_Directory()
        {
            using var client = factory.CreateClient();

            Assert.True(Directory.Exists(factory.DataDirectory));
        }
    }
}
