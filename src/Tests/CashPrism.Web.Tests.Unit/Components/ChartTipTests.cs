using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class ChartTipTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<ChartTip> RenderWith(ChartTipAnchor anchor = ChartTipAnchor.Centre)
            => context.Render<ChartTip>(parameters => parameters
                .Add(p => p.Title, "Wohnen · Jun 26")
                .Add(p => p.Value, "490 €")
                .Add(p => p.Colour, "var(--cp-prism-blue)")
                .Add(p => p.Anchor, anchor));

        [Fact]
        public void Names_The_Mark_And_Gives_Its_Figure()
        {
            var tip = RenderWith();

            Assert.Equal(
                ("Wohnen · Jun 26", "490 €"),
                (tip.Find(".cp-chart-tip__title").TextContent.Trim(), tip.Find(".cp-chart-tip__value").TextContent));
        }

        [Fact]
        public void Shows_The_Colour_Of_The_Mark_Beside_Its_Name()
            => Assert.Equal(
                "--c:var(--cp-prism-blue)",
                RenderWith().Find(".cp-chart-tip__title .cp-legend__swatch").GetAttribute("style"));

        [Fact]
        public void Sits_Above_The_Mark()
            => Assert.Equal("-80", RenderWith().Find("foreignObject").GetAttribute("y"));

        [Theory]
        [InlineData(ChartTipAnchor.Centre, "-130", "cp-chart-tip--centre")]
        [InlineData(ChartTipAnchor.Start, "-24", "cp-chart-tip--start")]
        [InlineData(ChartTipAnchor.End, "-236", "cp-chart-tip--end")]
        public void Leans_Inwards_At_The_Edges_Of_The_Chart(ChartTipAnchor anchor, string x, string modifier)
        {
            var tip = RenderWith(anchor);

            Assert.Equal(x, tip.Find("foreignObject").GetAttribute("x"));
            Assert.Contains(modifier, tip.Find(".cp-chart-tip").ClassList);
        }
    }
}
