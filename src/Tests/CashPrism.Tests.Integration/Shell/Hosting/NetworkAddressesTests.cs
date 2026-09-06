using System.Net;
using CashPrism.Shell.Hosting;

namespace CashPrism.Tests.Integration.Shell.Hosting;

/// <summary>
/// The banner may only advertise addresses another device in the home network
/// can reach. Everything else — loopback, link-local, IPv6 — is noise a person
/// would have to filter out by hand.
/// </summary>
public sealed class NetworkAddressesTests
{
    public sealed class FilterPrivateIPv4
    {
        [Theory]
        [InlineData("192.168.1.5")]
        [InlineData("10.0.0.5")]
        [InlineData("172.16.0.5")]
        [InlineData("172.31.255.254")]
        public void Keeps_A_Private_IPv4_Address(string address)
        {
            var kept = NetworkAddresses.FilterPrivateIPv4([IPAddress.Parse(address)]);

            Assert.Equal(address, Assert.Single(kept).ToString());
        }

        [Theory]
        [InlineData("172.15.0.1")]
        [InlineData("172.32.0.1")]
        [InlineData("169.254.1.1")]
        [InlineData("127.0.0.1")]
        [InlineData("fe80::1")]
        public void Drops_An_Address_Outside_The_Private_IPv4_Ranges(string address)
        {
            var kept = NetworkAddresses.FilterPrivateIPv4([IPAddress.Parse(address)]);

            Assert.Empty(kept);
        }

        [Fact]
        public void Keeps_The_Order_Of_The_Input()
        {
            var kept = NetworkAddresses.FilterPrivateIPv4(
            [
                IPAddress.Parse("192.168.1.5"),
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("10.0.0.5"),
            ]);

            Assert.Equal(["192.168.1.5", "10.0.0.5"], kept.Select(a => a.ToString()));
        }
    }
}
