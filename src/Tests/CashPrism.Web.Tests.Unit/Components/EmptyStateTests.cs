using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class EmptyStateTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Shows_The_Title_The_Text_And_The_Action()
        {
            var empty = context.Render<EmptyState>(parameters => parameters
                .Add(p => p.Title, "Nichts gefunden")
                .Add(p => p.Text, "Versuch es mit weniger Filtern.")
                .Add(p => p.Actions, "<button>Filter zurücksetzen</button>"));

            Assert.Equal("Nichts gefunden", empty.Find("h2").TextContent);
            Assert.Equal("Versuch es mit weniger Filtern.", empty.Find(".cp-empty__text").TextContent);
            Assert.Equal("Filter zurücksetzen", empty.Find(".cp-empty__actions button").TextContent);
        }

        [Fact]
        public void Draws_The_Given_Icon_In_The_Tile()
        {
            var empty = context.Render<EmptyState>(parameters => parameters
                .Add(p => p.Title, "Noch nichts importiert.")
                .Add(p => p.Icon, "upload"));

            Assert.NotNull(empty.Find(".cp-empty__icon .icon-upload"));
        }

        [Fact]
        public void Leaves_Out_Text_And_Actions_When_There_Are_None()
        {
            var empty = context.Render<EmptyState>(parameters => parameters.Add(p => p.Title, "Noch nichts importiert."));

            Assert.Empty(empty.FindAll(".cp-empty__text, .cp-empty__actions"));
        }
    }
}
