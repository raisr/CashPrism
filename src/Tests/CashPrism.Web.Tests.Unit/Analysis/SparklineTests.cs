using Bunit;
using CashPrism.Web.Analysis;

namespace CashPrism.Web.Tests.Unit.Analysis;

public sealed class SparklineTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<Sparkline> RenderWith(params long[] values)
            => context.Render<Sparkline>(parameters => parameters
                .Add(p => p.Values, values)
                .Add(p => p.Colour, "var(--cp-prism-violet)")
                .Add(p => p.Width, 104)
                .Add(p => p.Height, 24));

        [Fact]
        public void Draws_The_Highest_Value_At_The_Top_And_The_Lowest_At_The_Bottom()
        {
            var sparkline = RenderWith(0, 100, 50);

            Assert.Equal("M2,22 L52,2 L102,12", sparkline.Find(".cp-sparkline__line").GetAttribute("d"));
        }

        [Fact]
        public void Draws_Even_Values_As_A_Flat_Line()
        {
            var sparkline = RenderWith(40, 40);

            Assert.Equal("M2,2 L102,2", sparkline.Find(".cp-sparkline__line").GetAttribute("d"));
        }

        [Fact]
        public void Marks_The_Latest_Value_With_A_Dot()
        {
            var sparkline = RenderWith(0, 100, 50);

            var dot = sparkline.Find(".cp-sparkline__dot");
            Assert.Equal(("102", "12"), (dot.GetAttribute("cx"), dot.GetAttribute("cy")));
        }

        [Fact]
        public void Draws_Nothing_Without_Values()
        {
            var sparkline = RenderWith();

            Assert.Empty(sparkline.FindAll("path"));
        }
    }
}
