using Bunit;
using CashPrism.Web.Layout;

namespace CashPrism.Web.Tests.Unit.Layout;

public sealed class PageFrameTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Sets_The_Title_As_The_Focusable_Heading()
        {
            var frame = context.Render<PageFrame>(parameters => parameters.Add(p => p.Title, "Buchungen"));

            var heading = frame.Find("h1");
            Assert.Equal("Buchungen", heading.TextContent);
            Assert.Equal("-1", heading.GetAttribute("tabindex"));
        }

        [Fact]
        public void Places_The_Actions_Beside_The_Heading()
        {
            var frame = context.Render<PageFrame>(parameters => parameters
                .Add(p => p.Title, "Übersicht")
                .Add(p => p.Actions, "<button>6 Monate</button>"));

            Assert.Equal("6 Monate", frame.Find(".cp-pagehead__actions button").TextContent);
        }

        [Fact]
        public void Leaves_Out_The_Actions_When_There_Are_None()
        {
            var frame = context.Render<PageFrame>(parameters => parameters.Add(p => p.Title, "Übersicht"));

            Assert.Empty(frame.FindAll(".cp-pagehead__actions"));
        }
    }
}
