using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CashPrism.Tests.Integration.Shell;

/// <summary>
/// Hosts the composition root for the integration tests. It overrides the two
/// hosting settings that have a side effect outside the process: no browser
/// window opens, and the data directory is a throwaway folder outside the
/// repository. This is the same seam that will carry the throwaway database.
/// </summary>
public sealed class CashPrismWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>The throwaway data directory this host writes into.</summary>
    public string DataDirectory { get; } = Path.Combine(
        Path.GetTempPath(),
        "cashprism-tests",
        Guid.NewGuid().ToString("n"));

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Hosting:LaunchBrowser"] = "false",
                ["Hosting:DataDirectory"] = DataDirectory,
            }));

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(DataDirectory))
        {
            Directory.Delete(DataDirectory, recursive: true);
        }
    }
}
