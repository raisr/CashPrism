using Bunit;
using CashPrism.Web.Layout;
using CashPrism.Web.Localisation;
using CashPrism.Web.Theme;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Layout;

public sealed class MainLayoutTests
{
    public sealed class OnAfterRenderAsync : IAsyncLifetime
    {
        // MudThemeProvider asks the browser through this function.
        private const string SystemDarkMode = "mudThemeProvider.isDarkMode";

        private readonly BunitContext context = new();

        public OnAfterRenderAsync()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddMudServices();
            context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Starts_Dark_When_The_System_Is_Dark()
        {
            context.JSInterop.Setup<bool>(SystemDarkMode).SetResult(true);

            var layout = context.Render<MainLayout>();

            layout.WaitForAssertion(() => Assert.True(layout.FindComponent<ThemeAttribute>().Instance.IsDarkMode));
        }

        [Fact]
        public void Starts_Light_When_The_System_Is_Light()
        {
            context.JSInterop.Setup<bool>(SystemDarkMode).SetResult(false);

            var layout = context.Render<MainLayout>();

            Assert.False(layout.FindComponent<ThemeAttribute>().Instance.IsDarkMode);
        }

        [Fact]
        public void Keeps_A_Choice_Made_Before_The_System_Answered()
        {
            var systemAnswer = context.JSInterop.Setup<bool>(SystemDarkMode);
            var layout = context.Render<MainLayout>();
            layout.Find("button[aria-label='Zwischen hellem und dunklem Design wechseln']").Click();

            systemAnswer.SetResult(false);

            Assert.True(layout.FindComponent<ThemeAttribute>().Instance.IsDarkMode);
        }
    }

    public sealed class ToggleSidebar : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public ToggleSidebar()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddMudServices();
            context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Starts_With_The_Full_Drawer()
        {
            var layout = context.Render<MainLayout>();

            Assert.Empty(layout.FindAll(".cp-sidebar--collapsed"));
            Assert.Empty(layout.FindAll(".mud-layout.cp-shell--rail"));
        }

        [Fact]
        public void Collapses_The_Drawer_To_The_Rail()
        {
            var layout = context.Render<MainLayout>();

            layout.Find("button[aria-label='Navigation ausblenden']").Click();

            Assert.NotNull(layout.Find(".cp-sidebar--collapsed"));
            Assert.NotNull(layout.Find(".mud-layout.cp-shell--rail"));
        }

        [Fact]
        public void Expands_The_Rail_Back_To_The_Full_Drawer()
        {
            var layout = context.Render<MainLayout>();
            layout.Find("button[aria-label='Navigation ausblenden']").Click();

            layout.Find("button[aria-label='Navigation einblenden']").Click();

            Assert.Empty(layout.FindAll(".cp-sidebar--collapsed"));
            Assert.Empty(layout.FindAll(".mud-layout.cp-shell--rail"));
        }
    }
}
