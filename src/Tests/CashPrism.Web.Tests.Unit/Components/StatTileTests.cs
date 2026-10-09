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

        private IRenderedComponent<StatTile> RenderWith(StatTrend trend, bool good = true, string? delta = "+2 %")
            => context.Render<StatTile>(parameters => parameters
                .Add(p => p.Label, "Einnahmen im September")
                .Add(p => p.Trend, trend)
                .Add(p => p.DeltaGood, good)
                .Add(p => p.Delta, delta)
                .Add(p => p.DeltaLabel, "ggü. Vormonat")
                .AddChildContent("4.082,50 €"));

        private static string TileIcon(IRenderedComponent<StatTile> tile)
            => tile.Find(".cp-stat > .cp-caticon .cp-icon").ClassList.Single(name => name.StartsWith("icon-", StringComparison.Ordinal));

        private static string? TileColour(IRenderedComponent<StatTile> tile)
            => tile.Find(".cp-stat > .cp-caticon").GetAttribute("style")?.Split(';')[0];

        [Fact]
        public void Shows_The_Label_And_The_Figure()
        {
            var tile = RenderWith(StatTrend.Up);

            Assert.Equal(
                ("Einnahmen im September", "4.082,50 €"),
                (tile.Find(".cp-stat__label").TextContent, tile.Find(".cp-stat__value").TextContent));
        }

        [Fact]
        public void Points_The_Tile_Up_In_Green_When_The_Figure_Rose_For_The_Better()
        {
            var tile = RenderWith(StatTrend.Up, good: true);

            Assert.Equal(("icon-arrow-up-right", "--c:var(--cp-income)"), (TileIcon(tile), TileColour(tile)));
        }

        [Fact]
        public void Points_The_Tile_Up_In_Red_When_The_Figure_Rose_For_The_Worse()
        {
            var tile = RenderWith(StatTrend.Up, good: false);

            Assert.Equal(("icon-arrow-up-right", "--c:var(--cp-expense)"), (TileIcon(tile), TileColour(tile)));
        }

        [Fact]
        public void Points_The_Tile_Down_When_The_Figure_Fell()
        {
            var tile = RenderWith(StatTrend.Down, good: true);

            Assert.Equal(("icon-arrow-down-right", "--c:var(--cp-income)"), (TileIcon(tile), TileColour(tile)));
        }

        [Fact]
        public void Points_The_Tile_Ahead_In_Grey_When_There_Is_Nothing_To_Compare_With()
        {
            var tile = RenderWith(StatTrend.Unknown, delta: null);

            Assert.Equal(("icon-arrow-right", "--c:var(--cp-prism-slate)"), (TileIcon(tile), TileColour(tile)));
        }

        [Fact]
        public void Colours_A_Bad_Change_As_Bad()
        {
            var tile = RenderWith(StatTrend.Up, good: false);

            Assert.True(tile.Find(".cp-stat__delta b").ClassList.Contains("cp-delta--bad"));
        }

        [Fact]
        public void Leaves_An_Unchanged_Figure_Uncoloured()
        {
            var tile = RenderWith(StatTrend.Flat, delta: "0 %");

            Assert.Empty(tile.Find(".cp-stat__delta b").ClassList);
        }

        [Fact]
        public void Shows_No_Change_When_There_Is_Nothing_To_Compare_With()
        {
            var tile = RenderWith(StatTrend.Unknown, delta: null);

            Assert.Empty(tile.FindAll(".cp-stat__delta"));
        }
    }
}
