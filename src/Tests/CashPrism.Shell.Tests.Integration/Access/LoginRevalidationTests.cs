using CashPrism.Application.Access;
using CashPrism.Web.Access;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// The login of an open page against a password that changes underneath it,
/// with the real database and the provider the host wired. An open page holds
/// one provider for as long as it is open, as each scope here does.
/// </summary>
public sealed class LoginRevalidationTests
{
    public sealed class ValidateAuthenticationStateAsync : IAsyncLifetime
    {
        // One check, with room for a slow machine.
        private static readonly TimeSpan Patience = LoginRevalidation.Interval * 2;

        private const string TheNewPassword = "heftklammer pferd batterie";

        private readonly CashPrismWebApplicationFactory factory = new();
        private readonly TaskCompletionSource<AuthenticationState> changed =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private AsyncServiceScope page;

        public async Task InitializeAsync()
        {
            await factory.SetPasswordAsync();

            await using var scope = factory.Services.CreateAsyncScope();
            var credential = await scope.ServiceProvider.GetRequiredService<ICredentialStore>().GetAsync();

            page = factory.Services.CreateAsyncScope();
            var authentication = page.ServiceProvider.GetRequiredService<AuthenticationStateProvider>();

            // What the host does when a page connects: hand over the login of
            // the request that opened it. That announces itself as a change, so
            // listening starts only afterwards.
            ((IHostEnvironmentAuthenticationStateProvider)authentication).SetAuthenticationState(
                Task.FromResult(new AuthenticationState(AccessCookie.CreatePrincipal(credential!.Generation))));
            authentication.AuthenticationStateChanged += async state => changed.TrySetResult(await state);
        }

        public async Task DisposeAsync()
        {
            await page.DisposeAsync();
            factory.Dispose();
        }

        [Fact]
        public async Task Ends_The_Login_Of_An_Open_Page_Once_The_Password_Changed()
        {
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<PasswordChange>()
                    .ChangeAsync(CashPrismWebApplicationFactory.Password, TheNewPassword);
            }

            var state = await changed.Task.WaitAsync(Patience);

            Assert.False(state.User.Identity?.IsAuthenticated);
        }

        [Fact]
        public async Task Keeps_The_Login_Of_An_Open_Page_While_The_Password_Is_Unchanged()
        {
            var finished = await Task.WhenAny(changed.Task, Task.Delay(Patience));

            Assert.NotSame(changed.Task, finished);
        }
    }
}
