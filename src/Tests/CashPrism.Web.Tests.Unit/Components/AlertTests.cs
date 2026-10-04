using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class AlertTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Theory]
        [InlineData(AlertTone.Info, "cp-alert--info", "icon-info")]
        [InlineData(AlertTone.Success, "cp-alert--success", "icon-circle-check")]
        [InlineData(AlertTone.Warning, "cp-alert--warning", "icon-triangle-alert")]
        [InlineData(AlertTone.Danger, "cp-alert--danger", "icon-circle-alert")]
        public void Draws_Each_Tone_With_Its_Colour_And_Glyph(AlertTone tone, string toneClass, string glyph)
        {
            var alert = context.Render<Alert>(parameters => parameters
                .Add(p => p.Tone, tone)
                .Add(p => p.Title, "Fertig!"));

            Assert.True(alert.Find(".cp-alert").ClassList.Contains(toneClass));
            Assert.NotNull(alert.Find($".cp-alert > .{glyph}"));
        }

        [Fact]
        public void Lists_The_Details_Under_The_Headline()
        {
            var alert = context.Render<Alert>(parameters => parameters
                .Add(p => p.Title, "Das ist keine Finanzguru-Datei")
                .Add(p => p.Details, ["Im Export fehlen diese Spalten: Tags", "Spalte „Betrag“ in Zeile 4 …"]));

            Assert.Equal(
                ["Im Export fehlen diese Spalten: Tags", "Spalte „Betrag“ in Zeile 4 …"],
                alert.FindAll(".cp-alert__body li").Select(item => item.TextContent));
        }

        [Fact]
        public void Writes_A_Single_Detail_As_A_Sentence_Rather_Than_A_List()
        {
            var alert = context.Render<Alert>(parameters => parameters
                .Add(p => p.Title, "Diese Datei kennen wir schon")
                .Add(p => p.Details, ["Sie wurde bereits eingelesen – es hat sich nichts geändert."]));

            Assert.Equal(
                "Sie wurde bereits eingelesen – es hat sich nichts geändert.",
                alert.Find("p.cp-alert__body").TextContent);
            Assert.Empty(alert.FindAll("li"));
        }

        [Fact]
        public void Leaves_The_List_Out_Without_Details()
        {
            var alert = context.Render<Alert>(parameters => parameters.Add(p => p.Title, "Fertig!"));

            Assert.Empty(alert.FindAll(".cp-alert__body"));
        }
    }
}
