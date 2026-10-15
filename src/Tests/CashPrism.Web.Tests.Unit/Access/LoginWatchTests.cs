using Bunit;
using Bunit.TestDoubles;
using CashPrism.Web.Access;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Access;

public sealed class LoginWatchTests
{
    public sealed class Render : IAsyncLifetime
    {
        private const string Page = "http://localhost/bookings?page=2";

        private readonly BunitContext context = new();
        private readonly BunitAuthorizationContext authorization;

        public Render()
        {
            authorization = context.AddAuthorization();
            authorization.SetAuthorized("CashPrism");
            context.Services.GetRequiredService<NavigationManager>().NavigateTo(Page);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<LoginWatch> RenderWatch()
            => context.Render<LoginWatch>(parameters => parameters
                .AddChildContent("<p class=\"data\">Kontostand</p>"));

        [Fact]
        public void Shows_The_Page_While_The_Login_Counts()
        {
            var watch = RenderWatch();

            Assert.Single(watch.FindAll(".data"));
        }

        [Fact]
        public void Hides_The_Page_Once_The_Login_Ended()
        {
            var watch = RenderWatch();

            authorization.SetNotAuthorized();

            watch.WaitForAssertion(() => Assert.Empty(watch.FindAll(".data")));
        }

        [Fact]
        public void Reloads_The_Current_Page_From_The_Server_Once_The_Login_Ended()
        {
            var watch = RenderWatch();
            var navigation = context.Services.GetRequiredService<BunitNavigationManager>();

            authorization.SetNotAuthorized();

            watch.WaitForAssertion(() =>
            {
                var reload = navigation.History.First();
                Assert.Equal(Page, reload.Uri);
                Assert.True(reload.Options.ForceLoad);
            });
        }
    }
}
