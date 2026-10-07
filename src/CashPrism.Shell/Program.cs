using System.Globalization;
using System.Security.Cryptography;
using CashPrism.Application.Access;
using CashPrism.Application.Imports;
using CashPrism.Application.Persistence;
using CashPrism.Infrastructure.Configuration;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.Shell.Hosting;
using CashPrism.Web.Configuration;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.DataProtection;

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

    /// <summary>
    /// The data protection key ring, inside the data directory, so the directory
    /// carries everything a running CashPrism needs: a container recreated on the
    /// same volume, or a data directory moved to another machine, keeps it.
    /// </summary>
    private const string KeysDirectoryName = "keys";

    /// <summary>
    /// Isolates the key ring's payloads from other applications. Left to itself,
    /// ASP.NET Core derives it from the content root, so the same keys would no
    /// longer read what they protected once a release is unpacked into another
    /// folder.
    /// </summary>
    private const string DataProtectionApplicationName = "CashPrism";

    private Program()
    {
    }

    /// <summary>
    /// Builds and runs the host. Returns a non-zero exit code when the database
    /// is already in use by another instance, or the configured port is already
    /// taken.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        ApplyUiCulture();

        var hostArgs = HostingCommandLine.Expand(args);

        // The content root is where the executable lies, not the working
        // directory: appsettings.json and wwwroot sit next to the executable,
        // and a double-click on macOS, a shortcut or a terminal elsewhere starts
        // it in some other directory. Every static asset was then served empty.
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = hostArgs,
            ContentRootPath = AppContext.BaseDirectory,
        });

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

        // The consequence of listening on every interface over plain HTTP, to carry
        // into the shared password, is that credentials travel the home network
        // unencrypted.
        builder.WebHost.UseUrls(hosting.ListenUrl());

        // The host does this by itself in Development only. Without it, an
        // executable started out of a build rather than a publish output looks
        // for its stylesheets and scripts under a wwwroot that only publish
        // creates, and serves every one of them empty. A publish output carries
        // no runtime manifest, so there this changes nothing.
        builder.WebHost.UseStaticWebAssets();

        var dataDirectory = hosting.ResolveDataDirectory();

        // Before the database is opened, not after: SQLite will not create a file
        // in a directory that does not exist.
        Directory.CreateDirectory(dataDirectory);

        var databaseFile = Path.Combine(dataDirectory, DatabaseFileName);

        var dataProtection = builder.Services.AddDataProtection()
            .SetApplicationName(DataProtectionApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDirectory, KeysDirectoryName)));

        // The default location is encrypted with DPAPI on Windows; naming the
        // location drops that, so it is asked for again here. Elsewhere the keys
        // stay unencrypted, like the database next to them.
        if (OperatingSystem.IsWindows())
        {
            dataProtection.ProtectKeysWithDpapi();
        }

        builder.Services.AddCashPrismWeb();
        builder.Services.AddCashPrismPersistence(databaseFile);

        // Which external format an import reads is a decision of the composition
        // root, so it is wired here rather than behind an extension method in the
        // reader's own project — that would cost it a dependency on the container
        // to save two lines. A second source replaces the first of these two.
        builder.Services.AddScoped<IImportSource, FinanzguruImportSource>();
        builder.Services.AddScoped<Importer>();

        // A new code on every start, made here because it is random: until a
        // password is set, the banner prints it, and only who sees the console
        // or the container's log can set one.
        builder.Services.AddSingleton(
            new SetupCode(RandomNumberGenerator.GetString(SetupCode.Alphabet, SetupCode.Length)));
        builder.Services.AddScoped<PasswordSetup>();
        builder.Services.AddScoped<PasswordCheck>();

        // One for the process: wrong passwords are counted across every request.
        builder.Services.AddSingleton<LoginThrottle>();

        var app = builder.Build();

        app.MapCashPrismWeb();

        // Taken after the host is built and before the database is opened:
        // everything above only reads configuration, and the migration below is
        // the first line that writes to the file. The guard is held for as long
        // as this method runs, which is as long as the server serves.
        using var singleInstance = SingleInstanceGuard.TryAcquire(databaseFile);

        if (singleInstance is null)
        {
            // A stack trace in a window opened by a double-click helps nobody.
            Console.Error.WriteLine(
                $"CashPrism cannot start: another instance is already using {databaseFile}.");
            Console.Error.WriteLine(
                "Stop the other instance and try again.");

            return 1;
        }

        // The schema is brought up to date before the server listens, so no request
        // can arrive at a database this build does not fit.
        bool setupPending;

        await using (var scope = app.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync();

            setupPending = await scope.ServiceProvider.GetRequiredService<PasswordSetup>().IsPendingAsync();
        }

        // Printed only while it is worth something: once a password is set, the
        // code opens nothing.
        var setupCode = setupPending ? app.Services.GetRequiredService<SetupCode>() : null;

        // Set by every official .NET container image. Inside one, the addresses
        // of the machine are those of the container network, which no other
        // device in the house can reach.
        var runningInContainer = builder.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER");

        // Only once the server actually listens is the address worth printing.
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            StartBanner.Print(runningInContainer
                ? StartBanner.ComposeForContainer(hosting.Port, setupCode)
                : StartBanner.Compose(hosting.Port, NetworkAddresses.Local(), setupCode));

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
