using Bunit;
using CashPrism.Web.Network;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Network;

public sealed class ReachabilityCardTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render() => context.Services.AddLocalization();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<ReachabilityCard> RenderWith(Reachability reachability)
            => context.Render<ReachabilityCard>(parameters => parameters.Add(p => p.Reachability, reachability));

        [Fact]
        public void Lists_Every_Address()
        {
            var card = RenderWith(new Reachability(5099, InContainer: false, ["http://192.168.1.5:5099", "http://10.0.0.5:5099"]));

            Assert.Equal(
                ["http://192.168.1.5:5099", "http://10.0.0.5:5099"],
                card.FindAll(".cp-settings-reach__urls li").Select(item => item.TextContent));
        }

        [Fact]
        public void Says_So_When_No_Network_Reaches_It()
        {
            var card = RenderWith(new Reachability(5080, InContainer: false, []));

            Assert.Equal(
                "Dieser Rechner ist gerade mit keinem Heimnetz verbunden. Andere Geräte erreichen CashPrism erst, wenn er es ist.",
                card.Find(".cp-settings-reach__note").TextContent);
        }

        [Fact]
        public void Names_The_Port_And_Leaves_The_Address_To_The_Host_In_A_Container()
        {
            var card = RenderWith(new Reachability(5080, InContainer: true, []));

            Assert.Contains("hört dort auf Port 5080", card.Find(".cp-settings-reach__note").TextContent, StringComparison.Ordinal);
            Assert.Empty(card.FindAll(".cp-settings-reach__urls"));
        }
    }
}
