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
    private static readonly TimeSpan ReleasePatience = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan RetryInterval = TimeSpan.FromMilliseconds(100);

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
