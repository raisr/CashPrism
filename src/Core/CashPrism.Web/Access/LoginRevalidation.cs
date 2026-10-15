using CashPrism.Application.Access;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CashPrism.Web.Access;

/// <summary>
/// Asks again, while a page is open, whether the login it was opened with still
/// counts. The cookie is only checked on an HTTP request, and an open page sends
/// none: it talks over the connection it opened with. Without this, a password
/// change would not reach a page left open on another device until it reloads.
/// </summary>
public sealed class LoginRevalidation : RevalidatingServerAuthenticationStateProvider
{
    /// <summary>
    /// How long an open page goes on after the password changed, at most. Each
    /// check is a single-row read per open page, so it can be short.
    /// </summary>
    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory scopes;

    /// <summary>Creates the provider.</summary>
    /// <param name="loggerFactory">Logs a check that failed.</param>
    /// <param name="scopes">
    /// Opens a scope per check, so the check never shares a database context
    /// with the page it runs beside.
    /// </param>
    public LoginRevalidation(ILoggerFactory loggerFactory, IServiceScopeFactory scopes)
        : base(loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(scopes);

        this.scopes = scopes;
    }

    /// <inheritdoc />
    protected override TimeSpan RevalidationInterval => Interval;

    /// <inheritdoc />
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authenticationState);

        if (AccessCookie.ReadGeneration(authenticationState.User) is not { } generation)
        {
            return false;
        }

        await using var scope = scopes.CreateAsyncScope();
        var validation = scope.ServiceProvider.GetRequiredService<LoginValidation>();

        return await validation.IsCurrentAsync(generation, cancellationToken);
    }
}
