using System.Net;
using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

/// <summary>
/// The banner is the only instruction a person gets when they double-click the
/// executable, so the URLs it lists have to be complete and typeable.
/// </summary>
public sealed class StartBannerTests
{
    public sealed class Compose
    {
        [Fact]
        public void Lists_The_Loopback_Url_For_The_Configured_Port()
        {
            var lines = StartBanner.Compose(5099, []);

            Assert.Contains("  http://localhost:5099", lines);
        }

        [Fact]
        public void Lists_One_Url_Per_Private_Address()
        {
            var lines = StartBanner.Compose(
                5080,
                [IPAddress.Parse("192.168.1.5"), IPAddress.Parse("10.0.0.5")]);

            Assert.Contains("  http://192.168.1.5:5080", lines);
            Assert.Contains("  http://10.0.0.5:5080", lines);
        }

        [Fact]
        public void Ignores_An_Address_That_Is_Not_Reachable_From_The_Network()
        {
            var lines = StartBanner.Compose(5080, [IPAddress.Parse("169.254.1.1")]);

            Assert.DoesNotContain(lines, line => line.Contains("169.254.1.1"));
        }
    }
}
