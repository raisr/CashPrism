using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class StatTileTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<StatTile> RenderWith(string? delta, bool up = true, bool good = true)
            => context.Render<StatTile>(parameters => parameters
                .Add(p => p.Icon, "arrow-down-left")
                .Add(p => p.Colour, "var(--cp-income)")
                .Add(p => p.Label, "Einnahmen im September")
                .Add(p => p.Delta, delta)
                .Add(p => p.DeltaUp, up)
                .Add(p => p.DeltaGood, good)
                .Add(p => p.DeltaLabel, "ggü. Vormonat")
                .AddChildContent("4.082,50 €"));

        [Fact]
        public void Shows_The_Label_And_The_Figure()
        {
            var tile = RenderWith("+2 %");

            Assert.Equal(
                ("Einnahmen im September", "4.082,50 €"),
                (tile.Find(".cp-stat__label").TextContent, tile.Find(".cp-stat__value").TextContent));
        }

        [Fact]
        public void Points_The_Arrow_Down_When_The_Figure_Fell()
        {
            var tile = RenderWith("−25 %", up: false);

            Assert.NotNull(tile.Find(".cp-stat__delta .icon-arrow-down-right"));
        }

        [Fact]
        public void Colours_A_Bad_Change_As_Bad()
        {
            var tile = RenderWith("+34 %", good: false);

            Assert.True(tile.Find(".cp-stat__delta b").ClassList.Contains("cp-delta--bad"));
        }

        [Fact]
        public void Shows_No_Change_When_There_Is_Nothing_To_Compare_With()
        {
            var tile = RenderWith(delta: null);

            Assert.Empty(tile.FindAll(".cp-stat__delta"));
        }
    }
}
