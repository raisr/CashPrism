using CashPrism.Application.Persistence;
using CashPrism.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Infrastructure.Configuration;

/// <summary>
/// Registers the persistence layer with the application's service container.
/// Called by the host in <c>CashPrism.Shell</c>.
/// </summary>
public static class CashPrismPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds the database and the services that reach it.
    /// </summary>
    /// <param name="services">The container to register with.</param>
    /// <param name="databaseFilePath">
    /// Full path of the SQLite file. The host decides where it lives; the database
    /// must sit on a local disk, never on a network share, because SQLite locking
    /// over SMB is unreliable — see <c>Agents.md</c>.
    /// </param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceCollection AddCashPrismPersistence(
        this IServiceCollection services,
        string databaseFilePath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFilePath);

        services.AddDbContext<CashPrismDbContext>(options =>
            options.UseSqlite($"Data Source={databaseFilePath}"));

        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();

        return services;
    }
}
