using CashPrism.Shell.Hosting;
using CashPrism.Web.Configuration;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;

namespace CashPrism.Shell;

/// <summary>
/// The composition root. Besides wiring the web layer it carries the concerns of
/// an application someone starts by double-clicking it: the port it binds, the
/// addresses it advertises, its data directory and the browser it opens.
/// </summary>
public sealed class Program
{
    private Program()
    {
    }

    /// <summary>
    /// Builds and runs the host. Returns a non-zero exit code when the
    /// configured port is already taken.
    /// </summary>
    public static int Main(string[] args)
    {
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

        // Kestrel is configured before the container exists, so the port is read
        // straight from configuration here. Everything after Build() uses
        // IOptions.
        var port = builder.Configuration.GetValue(
            $"{HostingOptions.SectionName}:{nameof(HostingOptions.Port)}",
            new HostingOptions().Port);

        // Every device in the house reaches this, so binding to loopback is not
        // an option. HTTP only and on purpose: a self-signed certificate means a
        // warning on every phone and tablet. The consequence to carry into the
        // shared password is that credentials travel the home network
        // unencrypted.
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        builder.Services.AddCashPrismWeb();

        var app = builder.Build();

        app.MapCashPrismWeb();

        var hosting = app.Services.GetRequiredService<IOptions<HostingOptions>>().Value;

        Directory.CreateDirectory(hosting.ResolveDataDirectory());

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
            app.Run();
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
}
