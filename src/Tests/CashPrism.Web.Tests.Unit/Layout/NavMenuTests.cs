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
        [InlineData("/analysis", "Analyse")]
        [InlineData("/import", "Import")]
        [InlineData("/settings", "Einstellungen")]
        public void Marks_Only_The_Destination_Of_The_Current_Route(string route, string label)
        {
            context.Services.GetRequiredService<NavigationManager>().NavigateTo(route);

            var menu = context.Render<NavMenu>();

            var active = Assert.Single(menu.FindAll("a.cp-nav__item--on"));
            Assert.Equal(label, active.TextContent.Trim());
        }

        [Fact]
        public void Heads_The_Destinations_That_Analyse_With_Their_Section()
        {
            var menu = context.Render<NavMenu>();

            var section = menu.FindAll(".cp-sidebar__section")[0];
            Assert.Equal("Auswerten", section.TextContent);
            Assert.Equal("Analyse", section.NextElementSibling!.TextContent.Trim());
        }

        [Fact]
        public void Heads_The_Destinations_That_Bring_Data_In_With_Their_Section()
        {
            var menu = context.Render<NavMenu>();

            var section = menu.FindAll(".cp-sidebar__section")[1];
            Assert.Equal("Daten", section.TextContent);
            Assert.Equal("Import", section.NextElementSibling!.TextContent.Trim());
        }

        [Fact]
        public void Puts_The_Settings_In_The_Data_Section_After_The_Import()
        {
            var menu = context.Render<NavMenu>();

            var import = menu.FindAll(".cp-sidebar__section")[1].NextElementSibling!;
            Assert.Equal("Einstellungen", import.NextElementSibling!.TextContent.Trim());
        }

        [Fact]
        public void Names_Every_Destination_In_Its_Tooltip_On_The_Rail()
        {
            var menu = context.Render<NavMenu>(parameters => parameters.Add(p => p.Collapsed, true));

            var titles = menu.FindAll("a.cp-nav__item").Select(link => link.GetAttribute("title"));
            Assert.Equal(["Übersicht", "Buchungen", "Analyse", "Import", "Einstellungen"], titles);
        }

        [Fact]
        public void Shows_A_Count_Beside_The_Destination_It_Belongs_To()
        {
            var menu = context.Render<NavMenu>(parameters => parameters.Add(
                p => p.Counts,
                new Dictionary<NavigationItem, string> { [NavigationItems.Bookings] = "6.327" }));

            var count = Assert.Single(menu.FindAll(".cp-nav__count"));
            Assert.Equal("6.327", count.TextContent);
            Assert.Equal("/bookings", count.Closest("a")!.GetAttribute("href"));
        }

        [Fact]
        public void Shows_No_Count_Where_None_Is_Given()
        {
            var menu = context.Render<NavMenu>();

            Assert.Empty(menu.FindAll(".cp-nav__count"));
        }

        [Fact]
        public void Leaves_The_Tooltips_Out_On_The_Full_Drawer()
        {
            var menu = context.Render<NavMenu>();

            Assert.All(menu.FindAll("a.cp-nav__item"), link => Assert.False(link.HasAttribute("title")));
        }
    }
}
