using System.Net;
using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

public sealed class ShellReachabilityTests
{
    private static readonly IPAddress HomeNetwork = IPAddress.Parse("192.168.1.5");

    public sealed class Read
    {
        [Fact]
        public void Lists_The_Addresses_The_Start_Banner_Prints()
        {
            var reachability = new ShellReachability(5080, inContainer: false, () => [HomeNetwork]);

            Assert.Equal(["http://192.168.1.5:5080"], reachability.Read().Urls);
        }

        [Fact]
        public void Asks_For_The_Addresses_On_Every_Read()
        {
            var current = HomeNetwork;
            var reachability = new ShellReachability(5080, inContainer: false, () => [current]);
            reachability.Read();

            current = IPAddress.Parse("10.0.0.7");

            Assert.Equal(["http://10.0.0.7:5080"], reachability.Read().Urls);
        }

        [Fact]
        public void Names_No_Address_In_A_Container()
        {
            var reachability = new ShellReachability(5080, inContainer: true, () => [HomeNetwork]);

            var read = reachability.Read();

            Assert.Equal((5080, true), (read.Port, read.InContainer));
            Assert.Empty(read.Urls);
        }
    }
}
