using Bunit;
using CashPrism.Web.Preferences;
using Microsoft.AspNetCore.Http;

namespace CashPrism.Web.Tests.Unit.Preferences;

public sealed class CookiePreferenceStoreTests
{
    private const string ModulePath = "./_content/CashPrism.Web/js/preferences.js";

    private static CookiePreferenceStore StoreWith(BunitContext context, string? cookie)
    {
        var httpContext = new DefaultHttpContext();
        if (cookie is not null)
        {
            httpContext.Request.Headers.Cookie = $"{CookiePreferenceStore.CookieName}={cookie}";
        }

        return new CookiePreferenceStore(new HttpContextAccessor { HttpContext = httpContext }, context.JSInterop.JSRuntime);
    }

    public sealed class Theme : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Follows_The_System_Without_A_Cookie()
            => Assert.Equal(ThemePreference.System, StoreWith(context, cookie: null).Theme);

        [Fact]
        public void Reads_Dark_From_The_Cookie()
            => Assert.Equal(ThemePreference.Dark, StoreWith(context, "theme=dark").Theme);

        [Fact]
        public void Reads_Light_From_The_Cookie()
            => Assert.Equal(ThemePreference.Light, StoreWith(context, "theme=light&sidebar=collapsed").Theme);

        [Fact]
        public void Follows_The_System_When_The_Cookie_Names_An_Unknown_Theme()
            => Assert.Equal(ThemePreference.System, StoreWith(context, "theme=sepia").Theme);

        [Fact]
        public void Follows_The_System_Without_A_Request()
        {
            var store = new CookiePreferenceStore(new HttpContextAccessor(), context.JSInterop.JSRuntime);

            Assert.Equal(ThemePreference.System, store.Theme);
        }
    }

    public sealed class SidebarCollapsed : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Is_Expanded_Without_A_Cookie()
            => Assert.False(StoreWith(context, cookie: null).SidebarCollapsed);

        [Fact]
        public void Reads_Collapsed_From_The_Cookie()
            => Assert.True(StoreWith(context, "theme=dark&sidebar=collapsed").SidebarCollapsed);
    }

    public sealed class SetThemeAsync : IAsyncLifetime
    {
        private readonly BunitContext context = new();
        private readonly BunitJSModuleInterop module;

        public SetThemeAsync()
        {
            module = context.JSInterop.SetupModule(ModulePath);
            module.SetupVoid("writeCookie", _ => true).SetVoidResult();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public async Task Writes_The_Theme_Beside_The_Stored_Drawer()
        {
            var store = StoreWith(context, "sidebar=collapsed");

            await store.SetThemeAsync(ThemePreference.Dark);

            Assert.Equal(
                [CookiePreferenceStore.CookieName, "theme=dark&sidebar=collapsed", 31536000],
                module.VerifyInvoke("writeCookie").Arguments);
        }

        [Fact]
        public async Task Answers_With_The_Written_Theme()
        {
            var store = StoreWith(context, cookie: null);

            await store.SetThemeAsync(ThemePreference.Light);

            Assert.Equal(ThemePreference.Light, store.Theme);
        }
    }

    public sealed class SetSidebarCollapsedAsync : IAsyncLifetime
    {
        private readonly BunitContext context = new();
        private readonly BunitJSModuleInterop module;

        public SetSidebarCollapsedAsync()
        {
            module = context.JSInterop.SetupModule(ModulePath);
            module.SetupVoid("writeCookie", _ => true).SetVoidResult();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public async Task Writes_The_Drawer_Beside_The_Stored_Theme()
        {
            var store = StoreWith(context, "theme=light");

            await store.SetSidebarCollapsedAsync(true);

            Assert.Equal("theme=light&sidebar=collapsed", module.VerifyInvoke("writeCookie").Arguments[1]);
        }

        [Fact]
        public async Task Leaves_An_Expanded_Drawer_Out_Of_The_Cookie()
        {
            var store = StoreWith(context, "theme=dark&sidebar=collapsed");

            await store.SetSidebarCollapsedAsync(false);

            Assert.Equal("theme=dark", module.VerifyInvoke("writeCookie").Arguments[1]);
        }
    }
}
