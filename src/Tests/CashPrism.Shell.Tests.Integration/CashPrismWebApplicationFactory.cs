using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
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

        if (!disposing)
        {
            return;
        }

        // Disposing the host closes the contexts, but Microsoft.Data.Sqlite keeps
        // the connection in a pool and the pool keeps the file handle. Without
        // this the directory below cannot be deleted on Windows.
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(DataDirectory))
        {
            Directory.Delete(DataDirectory, recursive: true);
        }
    }
}
