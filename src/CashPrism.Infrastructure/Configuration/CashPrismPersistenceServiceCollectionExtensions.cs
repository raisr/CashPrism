using CashPrism.Application.Access;
using CashPrism.Application.Bookings;
using CashPrism.Application.CashFlow;
using CashPrism.Application.Imports;
using CashPrism.Application.Persistence;
using CashPrism.Application.Time;
using CashPrism.Infrastructure.Access;
using CashPrism.Infrastructure.Persistence;
using CashPrism.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        services.AddDbContext<CashPrismDbContext>(options => options
            .UseSqlite($"Data Source={databaseFilePath}")

            // SQLite rebuilds a table to change a column, and the rebuild turns
            // foreign keys off outside the migration's transaction; EF Core warns
            // about that on every database the migration runs on. The advice it
            // gives — split the migration — is for whoever writes migrations,
            // and the person starting CashPrism can act on none of it, so it is
            // logged where development sees it and a release does not.
            .ConfigureWarnings(warnings => warnings.Log(
                (RelationalEventId.NonTransactionalMigrationOperationWarning, LogLevel.Information))));

        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDataEraser, DataEraser>();
        services.AddScoped<IImportStore, ImportStore>();
        services.AddScoped<IBookingReader, BookingReader>();
        services.AddScoped<IImportRunReader, ImportRunReader>();
        services.AddScoped<ICashFlowReader, CashFlowReader>();
        services.AddScoped<ICredentialStore, CredentialStore>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
