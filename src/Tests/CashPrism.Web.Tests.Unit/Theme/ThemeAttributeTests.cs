using Bunit;
using CashPrism.Web.Theme;

namespace CashPrism.Web.Tests.Unit.Theme;

public sealed class ThemeAttributeTests
{
    private const string ModulePath = "./_content/CashPrism.Web/Theme/ThemeAttribute.razor.js";

    public sealed class OnAfterRenderAsync : IAsyncLifetime
    {
        private readonly BunitContext context = new();
        private readonly BunitJSModuleInterop module;

        public OnAfterRenderAsync()
        {
            module = context.JSInterop.SetupModule(ModulePath);
            module.SetupVoid("applyTheme", _ => true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Marks_The_Document_Dark_When_Dark_Mode_Is_On()
        {
            context.Render<ThemeAttribute>(parameters => parameters.Add(p => p.IsDarkMode, true));

            Assert.Equal([true], module.VerifyInvoke("applyTheme").Arguments);
        }

        [Fact]
        public void Marks_The_Document_Light_When_Dark_Mode_Is_Off()
        {
            context.Render<ThemeAttribute>(parameters => parameters.Add(p => p.IsDarkMode, false));

            Assert.Equal([false], module.VerifyInvoke("applyTheme").Arguments);
        }

        [Fact]
        public void Marks_The_Document_Again_When_Dark_Mode_Changes()
        {
            var component = context.Render<ThemeAttribute>(parameters => parameters.Add(p => p.IsDarkMode, false));

            component.Render(parameters => parameters.Add(p => p.IsDarkMode, true));

            Assert.Equal([true], module.VerifyInvoke("applyTheme", calledTimes: 2)[1].Arguments);
        }

        [Fact]
        public void Leaves_The_Document_Alone_When_Dark_Mode_Is_Unchanged()
        {
            var component = context.Render<ThemeAttribute>(parameters => parameters.Add(p => p.IsDarkMode, true));

            component.Render(parameters => parameters.Add(p => p.IsDarkMode, true));

            module.VerifyInvoke("applyTheme", calledTimes: 1);
        }
    }
}
