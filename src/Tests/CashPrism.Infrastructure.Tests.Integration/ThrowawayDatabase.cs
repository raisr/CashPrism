using CashPrism.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CashPrism.Infrastructure.Tests.Integration;

/// <summary>
/// A SQLite file created for one test run and deleted with it, migrated the same
/// way the application migrates its own: through the migrations, not through
/// <c>EnsureCreated</c>. A test that passed against a schema nobody ships proves
/// nothing.
/// </summary>
public sealed class ThrowawayDatabase : IAsyncDisposable
{
    private readonly string filePath = Path.Combine(
        Path.GetTempPath(),
        "cashprism-tests",
        $"{Guid.NewGuid():n}.db");

    /// <summary>
    /// Creates the file, its directory, and applies every migration.
    /// </summary>
    public static async Task<ThrowawayDatabase> CreateAsync()
    {
        var database = new ThrowawayDatabase();

        Directory.CreateDirectory(Path.GetDirectoryName(database.filePath)!);

        await database.MigrateAsync();

        return database;
    }

    /// <summary>
    /// Creates the file and applies the migrations up to and including
    /// <paramref name="upToMigration"/>, leaving the ones after it unapplied.
    /// That is how a test gets at a database as an earlier version of CashPrism
    /// left it, which is the only way to assert what a migration does to rows
    /// that are already there.
    /// </summary>
    /// <param name="upToMigration">
    /// The migration to stop at, by name without its timestamp — for example
    /// <c>ImportCountsAndBookingSource</c>.
    /// </param>
    public static async Task<ThrowawayDatabase> CreateAsync(string upToMigration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upToMigration);

        var database = new ThrowawayDatabase();

        Directory.CreateDirectory(Path.GetDirectoryName(database.filePath)!);

        await database.MigrateAsync(upToMigration);

        return database;
    }

    /// <summary>
    /// Brings the file up to the schema this build expects, the same way the
    /// application does on start.
    /// </summary>
    /// <param name="upToMigration">
    /// Where to stop, or <see langword="null"/> for every migration there is.
    /// </param>
    public async Task MigrateAsync(string? upToMigration = null)
    {
        await using var context = CreateContext();

        if (upToMigration is null)
        {
            await context.Database.MigrateAsync();

            return;
        }

        await context.GetService<IMigrator>().MigrateAsync(upToMigration);
    }

    /// <summary>
    /// A fresh context on the same file. Each call is its own unit of work, so a
    /// test can write through one and read through another and be sure the row came
    /// back from the database rather than from the change tracker.
    /// </summary>
    public CashPrismDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CashPrismDbContext>()
            .UseSqlite($"Data Source={filePath}")
            .Options;

        return new CashPrismDbContext(options);
    }

    /// <summary>Opens a raw connection, for asserting what a column actually holds.</summary>
    public SqliteConnection CreateConnection() => new($"Data Source={filePath}");

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        // The pool holds the file handle open, and on Windows that is enough to
        // make the delete below fail.
        SqliteConnection.ClearAllPools();

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return ValueTask.CompletedTask;
    }
}
