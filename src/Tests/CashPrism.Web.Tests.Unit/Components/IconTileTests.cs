using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class IconTileTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Tints_The_Tile_With_The_Colour_It_Is_Given()
        {
            var tile = context.Render<IconTile>(parameters => parameters
                .Add(p => p.Icon, "file-spreadsheet")
                .Add(p => p.Colour, "var(--cp-prism-green)")
                .Add(p => p.Size, 34));

            Assert.Equal("--c:var(--cp-prism-green);width:34px;height:34px", tile.Find(".cp-caticon").GetAttribute("style"));
            Assert.NotNull(tile.Find(".cp-caticon .icon-file-spreadsheet"));
        }

        [Fact]
        public void Is_A_Rounded_Square_Unless_Asked_To_Be_Round()
        {
            var square = context.Render<IconTile>(parameters => parameters
                .Add(p => p.Icon, "upload")
                .Add(p => p.Colour, "var(--cp-primary)"));
            var round = context.Render<IconTile>(parameters => parameters
                .Add(p => p.Icon, "upload")
                .Add(p => p.Colour, "var(--cp-primary)")
                .Add(p => p.Round, true));

            Assert.Empty(square.FindAll(".cp-caticon--round"));
            Assert.NotNull(round.Find(".cp-caticon--round"));
        }
    }
}
