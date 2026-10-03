using Bunit;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class CpIconTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Names_The_Glyph_As_An_Icon_Font_Class()
        {
            var icon = context.Render<CpIcon>(parameters => parameters.Add(p => p.Name, "wallet"));

            Assert.Equal("cp-icon icon-wallet", icon.Find("i").GetAttribute("class"));
        }

        [Fact]
        public void Appends_Further_Classes_After_The_Glyph()
        {
            var icon = context.Render<CpIcon>(parameters => parameters
                .Add(p => p.Name, "wallet")
                .Add(p => p.Class, "muted"));

            Assert.Equal("cp-icon icon-wallet muted", icon.Find("i").GetAttribute("class"));
        }

        [Fact]
        public void Sets_The_Font_Size_When_A_Size_Is_Given()
        {
            var icon = context.Render<CpIcon>(parameters => parameters
                .Add(p => p.Name, "wallet")
                .Add(p => p.Size, 16));

            Assert.Equal("font-size:16px", icon.Find("i").GetAttribute("style"));
        }

        [Fact]
        public void Leaves_The_Size_To_The_Stylesheet_When_None_Is_Given()
        {
            var icon = context.Render<CpIcon>(parameters => parameters.Add(p => p.Name, "wallet"));

            Assert.False(icon.Find("i").HasAttribute("style"));
        }

        [Fact]
        public void Is_Hidden_From_Assistive_Technology()
        {
            var icon = context.Render<CpIcon>(parameters => parameters.Add(p => p.Name, "wallet"));

            Assert.Equal("true", icon.Find("i").GetAttribute("aria-hidden"));
        }
    }
}
