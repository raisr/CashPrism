using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

public sealed class HostingOptionsTests
{
    public sealed class ListenUrl
    {
        /// <summary>
        /// The tests start the executable on loopback to keep the firewall
        /// quiet, so nothing else would notice if the shipped default narrowed.
        /// </summary>
        [Fact]
        public void Listens_On_Every_Interface_By_Default()
            => Assert.Equal("http://0.0.0.0:5080", new HostingOptions().ListenUrl());

        [Fact]
        public void Listens_On_The_Configured_Port()
            => Assert.Equal("http://0.0.0.0:5099", new HostingOptions { Port = 5099 }.ListenUrl());
    }
}
