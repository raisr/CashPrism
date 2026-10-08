using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class SegmentedTests
{
    private static readonly IReadOnlyList<SegmentedOption<int>> Spans =
    [
        new(6, "6 Monate"),
        new(12, "12 Monate"),
        new(24, "2 Jahre"),
    ];

    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Raises_The_Value_Of_The_Choice_That_Was_Clicked()
        {
            var selected = 12;
            var segmented = context.Render<Segmented<int>>(parameters => parameters
                .Add(p => p.Options, Spans)
                .Add(p => p.Value, selected)
                .Add(p => p.ValueChanged, value => selected = value));

            segmented.FindAll("button")[2].Click();

            Assert.Equal(24, selected);
        }

        [Fact]
        public void Marks_The_Selected_Choice_As_Pressed()
        {
            var segmented = context.Render<Segmented<int>>(parameters => parameters
                .Add(p => p.Options, Spans)
                .Add(p => p.Value, 12));

            var pressed = segmented.FindAll("button").Select(button => button.GetAttribute("aria-pressed"));
            Assert.Equal(["false", "true", "false"], pressed);
        }
    }
}
