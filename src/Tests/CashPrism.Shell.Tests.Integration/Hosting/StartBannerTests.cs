using System.Net;
using CashPrism.Application.Access;
using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

/// <summary>
/// The banner is the only instruction a person gets when they double-click the
/// executable, so the URLs it lists have to be complete and typeable.
/// </summary>
public sealed class StartBannerTests
{
    private static readonly SetupCode ACode = new("K7QFM2XP9HTR");

    public sealed class Compose
    {
        [Fact]
        public void Shows_The_Setup_Code_While_No_Password_Is_Set()
        {
            var lines = StartBanner.Compose(5080, [], ACode);

            Assert.Contains("  K7QF-M2XP-9HTR", lines);
        }

        [Fact]
        public void Shows_No_Setup_Code_Once_A_Password_Is_Set()
        {
            var lines = StartBanner.Compose(5080, [], setupCode: null);

            Assert.DoesNotContain(lines, line => line.Contains("setup code", StringComparison.Ordinal));
        }

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

    public sealed class ComposeForContainer
    {
        [Fact]
        public void Names_The_Port_The_Server_Listens_On()
        {
            var lines = StartBanner.ComposeForContainer(5080);

            Assert.Contains("Listening on port 5080 inside the container.", lines);
        }

        [Fact]
        public void Shows_How_The_Port_Is_Reached_From_The_Host()
        {
            var lines = StartBanner.ComposeForContainer(5080);

            Assert.Contains("  http://localhost:5080 when started with -p 5080:5080", lines);
        }

        // In a container the console is the log, and the log is the only place
        // the code can be read from.
        [Fact]
        public void Shows_The_Setup_Code_While_No_Password_Is_Set()
        {
            var lines = StartBanner.ComposeForContainer(5080, ACode);

            Assert.Contains("  K7QF-M2XP-9HTR", lines);
        }
    }
}
