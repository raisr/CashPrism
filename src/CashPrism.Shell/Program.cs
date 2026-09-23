using System.Globalization;
using CashPrism.Application.Persistence;
using CashPrism.Infrastructure.Configuration;
using CashPrism.Shell.Hosting;
using CashPrism.Web.Configuration;
using Microsoft.AspNetCore.Connections;

namespace CashPrism.Shell;

/// <summary>
/// The composition root. Besides wiring the web layer it carries the concerns of
/// an application someone starts by double-clicking it: the port it binds, the
/// addresses it advertises, its data directory and the browser it opens.
/// </summary>
public sealed class Program
{
    /// <summary>
    /// The database file, inside the data directory. The README promises it sits
    /// next to the application, so the name is fixed rather than configurable —
    /// what a person may move is the directory.
    /// </summary>
    private const string DatabaseFileName = "cashprism.db";

    private Program()
    {
    }

    /// <summary>
    /// Builds and runs the host. Returns a non-zero exit code when the
    /// configured port is already taken.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        ApplyUiCulture();

        var hostArgs = HostingCommandLine.Expand(args);

        var builder = WebApplication.CreateBuilder(hostArgs);

        builder.Configuration.AddCommandLine(hostArgs, HostingCommandLine.CreateSwitchMappings());

        builder.Services.Configure<HostingOptions>(
            builder.Configuration.GetSection(HostingOptions.SectionName));

        // The generic host logs a failed start with the full stack trace before
        // the exception reaches the catch below. That trace is duplicate
        // information in a window someone opened by double-clicking: anything
        // this code does not handle still surfaces through the runtime's
        // unhandled-exception output.
        builder.Logging.AddFilter("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.None);

        // Kestrel and the database are configured before the container exists, so
        // the settings are bound straight from configuration here. Everything after
        // Build() uses IOptions.
        var hosting = builder.Configuration.GetSection(HostingOptions.SectionName).Get<HostingOptions>()
            ?? new HostingOptions();

        // Every device in the house reaches this, so binding to loopback is not
        // an option. HTTP only and on purpose: a self-signed certificate means a
        // warning on every phone and tablet. The consequence to carry into the
        // shared password is that credentials travel the home network
        // unencrypted.
        builder.WebHost.UseUrls($"http://0.0.0.0:{hosting.Port}");

        var dataDirectory = hosting.ResolveDataDirectory();

        // Before the database is opened, not after: SQLite will not create a file
        // in a directory that does not exist.
        Directory.CreateDirectory(dataDirectory);

        builder.Services.AddCashPrismWeb();
        builder.Services.AddCashPrismPersistence(Path.Combine(dataDirectory, DatabaseFileName));

        var app = builder.Build();

        app.MapCashPrismWeb();

        // The schema is brought up to date before the server listens, so no request
        // can arrive at a database this build does not fit.
        await using (var scope = app.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync();
        }

        // Only once the server actually listens is the address worth printing.
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            StartBanner.Print(hosting.Port, NetworkAddresses.Local());

            if (hosting.LaunchBrowser)
            {
                BrowserLauncher.Open($"http://localhost:{hosting.Port}", app.Logger);
            }
        });

        try
        {
            await app.RunAsync();
        }
        catch (IOException exception) when (exception.InnerException is AddressInUseException)
        {
            // A stack trace in a window opened by a double-click helps nobody.
            Console.Error.WriteLine(
                $"CashPrism cannot start: port {hosting.Port} is already in use.");
            Console.Error.WriteLine(
                "Stop the other instance, or pick another port with --port <number>.");

            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Pins the process to German. The UI is written in German (see
    /// <c>Agents.md</c>), so the culture is a decision the host takes rather than
    /// something inherited from whichever machine the executable was
    /// double-clicked on — an English Windows would otherwise format amounts and
    /// dates one way while the labels next to them read another.
    /// </summary>
    /// <remarks>
    /// Setting the defaults for every thread is what a Blazor Server circuit
    /// needs: it outlives the request that created it, so per-request
    /// localisation middleware would not reach it.
    /// </remarks>
    private static void ApplyUiCulture()
    {
        var culture = new CultureInfo("de-DE");

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
