using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CashPrism.Tests.Integration.Shell;

/// <summary>
/// End-to-end smoke test for the composition root: the host must build and
/// serve requests with the current wiring (<c>AddCashPrismWeb</c> /
/// <c>MapCashPrismWeb</c>), even though no endpoint is mapped yet.
/// </summary>
public sealed class HostBootTests
{
    public sealed class Startup
    {
        [Fact]
        public async Task Host_Builds_And_Answers_Requests()
        {
            await using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient();

            using var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
