using CashPrism.Application.Access;
using CashPrism.Shell.Tests.Integration.Access;
using CashPrism.Web.Access;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Program = CashPrism.Shell.Program;

namespace CashPrism.Shell.Tests.Integration;

/// <summary>
/// Hosts the composition root for the integration tests. It overrides the two
/// hosting settings that have a side effect outside the process: no browser
/// window opens, and the data directory is a throwaway folder outside the
/// repository. This is the same seam that will carry the throwaway database.
/// </summary>
public sealed class CashPrismWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly TimeSpan ReleasePatience = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan RetryInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>The throwaway data directory this host writes into.</summary>
    public string DataDirectory { get; } = Path.Combine(
        Path.GetTempPath(),
        "cashprism-tests",
        Guid.NewGuid().ToString("n"));

    /// <summary>
    /// The environment the host runs in, or <c>null</c> for the Development the
    /// factory picks by default. Production is what an executable started
    /// outside Visual Studio runs as.
    /// </summary>
    public string? EnvironmentName { get; init; }

    /// <summary>
    /// Further configuration for the host, on top of the two settings it always
    /// overrides: what a command-line switch or the container image would set.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Configuration { get; init; } = new Dictionary<string, string?>();

    /// <summary>The password <see cref="SetPasswordAsync"/> sets.</summary>
    public const string Password = "korrekt pferd batterie";

    /// <summary>
    /// Sets <see cref="Password"/> through the setup the host wired, with the
    /// setup code of this start, unless a password is set already.
    /// </summary>
    public async Task SetPasswordAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var setup = scope.ServiceProvider.GetRequiredService<PasswordSetup>();

        if (await setup.IsPendingAsync())
        {
            await setup.SetAsync(Services.GetRequiredService<SetupCode>().Display, Password);
        }
    }

    /// <summary>
    /// A client that does not follow redirects, so a test sees where it was
    /// sent. It keeps cookies, so a login through it lasts.
    /// </summary>
    public HttpClient CreateClientThatStops()
        => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>
    /// A client that has logged in through the login page, with
    /// <see cref="Password"/> set first if it is not.
    /// </summary>
    public async Task<HttpClient> CreateSignedInClientAsync()
    {
        await SetPasswordAsync();

        var client = CreateClientThatStops();
        using var response = await HtmlForm.SubmitAsync(
            client,
            AccessPaths.Login,
            new Dictionary<string, string> { ["Input.Password"] = Password });

        if (response.Target() != ReturnUrl.Fallback)
        {
            client.Dispose();
            throw new InvalidOperationException($"The login did not succeed: {response.StatusCode}.");
        }

        return client;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>(Configuration)
            {
                ["Hosting:LaunchBrowser"] = "false",
                ["Hosting:DataDirectory"] = DataDirectory,
            }));

        if (EnvironmentName is not null)
        {
            builder.UseEnvironment(EnvironmentName);
        }

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        // Disposing the host closes the contexts, but Microsoft.Data.Sqlite keeps
        // the connection in a pool and the pool keeps the file handle. Without
        // this the directory below cannot be deleted on Windows. Only this
        // host's pool, under the connection string the host opened it with:
        // clearing every pool in the process would close the connections of
        // tests running beside this one.
        using (var connection = new SqliteConnection($"Data Source={Path.Combine(DataDirectory, "cashprism.db")}"))
        {
            SqliteConnection.ClearPool(connection);
        }

        DeleteDataDirectory();
    }

    /// <summary>
    /// Deletes <see cref="DataDirectory"/>, waiting for the single-instance lock
    /// to be let go. <c>Program.Main</c> holds it until <c>RunAsync</c> returns,
    /// and that happens on its own thread after the host has stopped — so the
    /// lock file can still be open for a moment when this runs. A lock that is
    /// never released still fails the test once the patience runs out.
    /// </summary>
    private void DeleteDataDirectory()
    {
        var giveUpAt = DateTime.UtcNow + ReleasePatience;

        while (true)
        {
            try
            {
                if (Directory.Exists(DataDirectory))
                {
                    Directory.Delete(DataDirectory, recursive: true);
                }

                return;
            }
            catch (IOException) when (DateTime.UtcNow < giveUpAt)
            {
                Thread.Sleep(RetryInterval);
            }
        }
    }
}
