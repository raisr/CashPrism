using Bunit;
using CashPrism.Web.Bookings;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class CategoryIconTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Tints_The_Tile_With_The_Colour_Of_The_Category()
        {
            var icon = context.Render<CategoryIcon>(parameters => parameters
                .Add(p => p.Category, CategoryStyle.Groceries));

            Assert.Contains("--c:var(--cp-prism-green)", icon.Find(".cp-caticon").GetAttribute("style"), StringComparison.Ordinal);
        }

        [Fact]
        public void Draws_The_Glyph_Of_The_Category()
        {
            var icon = context.Render<CategoryIcon>(parameters => parameters
                .Add(p => p.Category, CategoryStyle.Groceries));

            Assert.NotNull(icon.Find(".cp-caticon .icon-shopping-basket"));
        }

        [Fact]
        public void Is_Thirty_Six_Pixels_With_An_Eighteen_Pixel_Glyph_Unless_Told_Otherwise()
        {
            var icon = context.Render<CategoryIcon>(parameters => parameters
                .Add(p => p.Category, CategoryStyle.Housing));

            Assert.Contains("width:36px;height:36px", icon.Find(".cp-caticon").GetAttribute("style"), StringComparison.Ordinal);
            Assert.Equal("font-size:18px", icon.Find(".cp-icon").GetAttribute("style"));
        }

        [Fact]
        public void Sets_The_Glyph_At_Half_The_Tile()
        {
            var icon = context.Render<CategoryIcon>(parameters => parameters
                .Add(p => p.Category, CategoryStyle.Housing)
                .Add(p => p.Size, 52));

            Assert.Equal("font-size:26px", icon.Find(".cp-icon").GetAttribute("style"));
        }
    }
}
