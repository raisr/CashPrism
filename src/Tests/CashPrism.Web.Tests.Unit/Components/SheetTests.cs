using Bunit;
using CashPrism.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class SheetTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Shows_Nothing_While_Closed()
        {
            var sheet = context.Render<Sheet>(parameters => parameters
                .Add(p => p.Title, "Deutsche Bahn"));

            Assert.Empty(sheet.FindAll(".cp-sheet"));
            Assert.Empty(sheet.FindAll(".cp-sheet-scrim"));
        }

        [Fact]
        public void Shows_A_Labelled_Dialog_When_Open()
        {
            var sheet = context.Render<Sheet>(parameters => parameters
                .Add(p => p.Open, true)
                .Add(p => p.Title, "Deutsche Bahn")
                .Add(p => p.Subtitle, "Gestern"));

            var dialog = sheet.Find(".cp-sheet[role='dialog']");
            var title = sheet.Find($"#{dialog.GetAttribute("aria-labelledby")}");
            Assert.Equal("Deutsche Bahn", title.TextContent);
            Assert.Equal("Gestern", sheet.Find(".cp-sheet__subtitle").TextContent);
        }

        [Fact]
        public void Takes_The_Focus_When_It_Opens()
        {
            context.Render<Sheet>(parameters => parameters
                .Add(p => p.Open, true)
                .Add(p => p.Title, "Deutsche Bahn"));

            context.JSInterop.VerifyFocusAsyncInvoke();
        }

        [Theory]
        [InlineData("Escape", true)]
        [InlineData("Enter", false)]
        public void Asks_To_Be_Closed_On_Escape_Only(string key, bool closes)
        {
            var closed = false;
            var sheet = context.Render<Sheet>(parameters => parameters
                .Add(p => p.Open, true)
                .Add(p => p.Title, "Deutsche Bahn")
                .Add(p => p.OnClose, () => closed = true));

            sheet.Find(".cp-sheet").KeyDown(key);

            Assert.Equal(closes, closed);
        }

        [Fact]
        public void Asks_To_Be_Closed_By_A_Click_On_The_Dimmed_Page()
        {
            var closed = false;
            var sheet = context.Render<Sheet>(parameters => parameters
                .Add(p => p.Open, true)
                .Add(p => p.Title, "Deutsche Bahn")
                .Add(p => p.OnClose, () => closed = true));

            sheet.Find(".cp-sheet-scrim").Click();

            Assert.True(closed);
        }

        [Fact]
        public void Asks_To_Be_Closed_By_Its_Close_Button()
        {
            var closed = false;
            var sheet = context.Render<Sheet>(parameters => parameters
                .Add(p => p.Open, true)
                .Add(p => p.Title, "Deutsche Bahn")
                .Add(p => p.OnClose, () => closed = true));

            sheet.Find("button[aria-label='Schließen']").Click();

            Assert.True(closed);
        }
    }
}
