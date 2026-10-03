using Bunit;
using CashPrism.Web.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Layout;

public sealed class NavMenuTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render() => context.Services.AddLocalization();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Theory]
        [InlineData("/", "Übersicht")]
        [InlineData("/bookings", "Buchungen")]
        [InlineData("/import", "Import")]
        public void Marks_Only_The_Destination_Of_The_Current_Route(string route, string label)
        {
            context.Services.GetRequiredService<NavigationManager>().NavigateTo(route);

            var menu = context.Render<NavMenu>();

            var active = Assert.Single(menu.FindAll("a.cp-nav__item--on"));
            Assert.Equal(label, active.TextContent.Trim());
        }

        [Fact]
        public void Heads_The_Destinations_That_Bring_Data_In_With_Their_Section()
        {
            var menu = context.Render<NavMenu>();

            var section = Assert.Single(menu.FindAll(".cp-sidebar__section"));
            Assert.Equal("Daten", section.TextContent);
            Assert.Equal("Import", section.NextElementSibling!.TextContent.Trim());
        }
    }
}
