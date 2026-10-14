using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class TabsTests
{
    private static readonly IReadOnlyList<TabOption<string>> Views =
    [
        new("over", "Überblick"),
        new("bookings", "Buchungen", "412"),
    ];

    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Raises_The_Value_Of_The_Tab_That_Was_Clicked()
        {
            var selected = "over";
            var tabs = context.Render<Tabs<string>>(parameters => parameters
                .Add(p => p.Options, Views)
                .Add(p => p.Value, selected)
                .Add(p => p.ValueChanged, value => selected = value));

            tabs.FindAll("[role=tab]")[1].Click();

            Assert.Equal("bookings", selected);
        }

        [Fact]
        public void Marks_The_Selected_Tab_As_Selected()
        {
            var tabs = context.Render<Tabs<string>>(parameters => parameters
                .Add(p => p.Options, Views)
                .Add(p => p.Value, "bookings"));

            var selected = tabs.FindAll("[role=tab]").Select(tab => tab.GetAttribute("aria-selected"));
            Assert.Equal(["false", "true"], selected);
        }

        [Fact]
        public void Shows_A_Count_Only_Beside_The_Tab_That_Has_One()
        {
            var tabs = context.Render<Tabs<string>>(parameters => parameters
                .Add(p => p.Options, Views)
                .Add(p => p.Value, "over"));

            var counts = tabs.FindAll("[role=tab]").Select(tab => tab.QuerySelector(".cp-tab__count")?.TextContent);
            Assert.Equal([null, "412"], counts);
        }
    }
}
