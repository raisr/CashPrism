namespace CashPrism.AppHost;

/// <summary>
/// A development tool that builds the container image from the repository's
/// <c>Dockerfile</c> and starts it, with the Aspire dashboard showing the
/// running container, its logs and a link to the site. Declarations only; see
/// <c>docs/hosting.md</c>.
/// </summary>
public sealed class Program
{
    private Program()
    {
    }

    /// <summary>Declares the one resource and runs until stopped.</summary>
    public static async Task Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // The repository root, relative to this project: the image is built
        // with the same context as a `docker build .` there.
        builder.AddDockerfile("cashprism", "../../..")
            .WithHttpEndpoint(port: 5080, targetPort: 5080)
            .WithVolume("cashprism-data", "/data")
            .WithEnvironment("TZ", "Europe/Berlin");

        await builder.Build().RunAsync();
    }
}
