using CashPrism.Shell.Hosting;
using Microsoft.Extensions.Configuration;

namespace CashPrism.Shell.Tests.Integration.Hosting;

/// <summary>
/// The command-line provider silently mis-parses a valueless flag, so the two
/// switches are only safe in combination. These tests pin that combination.
/// </summary>
public sealed class HostingCommandLineTests
{
    public sealed class Expand
    {
        [Fact]
        public void Rewrites_The_Browser_Flag_Into_A_Key_Value_Argument()
        {
            var expanded = HostingCommandLine.Expand(["--no-browser"]);

            Assert.Equal(["--Hosting:LaunchBrowser=false"], expanded);
        }

        [Fact]
        public void Leaves_Every_Other_Argument_Untouched()
        {
            var expanded = HostingCommandLine.Expand(["--port", "5099"]);

            Assert.Equal(["--port", "5099"], expanded);
        }
    }

    public sealed class CreateSwitchMappings
    {
        private static IConfiguration Configure(params string[] args)
        {
            return new ConfigurationBuilder()
                .AddCommandLine(
                    HostingCommandLine.Expand(args),
                    HostingCommandLine.CreateSwitchMappings())
                .Build();
        }

        [Fact]
        public void Maps_The_Port_Switch_Onto_The_Hosting_Section()
        {
            var configuration = Configure("--port", "5099");

            Assert.Equal("5099", configuration["Hosting:Port"]);
        }

        [Fact]
        public void Keeps_The_Port_When_The_Browser_Flag_Comes_First()
        {
            var configuration = Configure("--no-browser", "--port", "5099");

            Assert.Equal("5099", configuration["Hosting:Port"]);
        }

        [Fact]
        public void Switches_The_Browser_Launch_Off_When_The_Flag_Comes_Last()
        {
            var configuration = Configure("--port", "5099", "--no-browser");

            Assert.Equal("false", configuration["Hosting:LaunchBrowser"]);
        }
    }
}
