using CashPrism.Domain.Access;
using CashPrism.Domain.Accounts;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// The database. One SQLite file on local disk, holding the bookings, the import
/// runs with the raw rows and account balances they brought, and the hash of the
/// shared password.
/// </summary>
/// <remarks>
/// The models carry no persistence attributes: everything the database needs to
/// know is in the configuration classes next to this one, which is what lets the
/// rules be read and tested without a database.
/// </remarks>
public sealed class CashPrismDbContext : DbContext
{
    /// <summary>
    /// Creates the context.
    /// </summary>
    /// <param name="options">The provider and connection, supplied by the host.</param>
    public CashPrismDbContext(DbContextOptions<CashPrismDbContext> options)
        : base(options)
    {
    }

    /// <summary>The stored bookings, keyed by fingerprint.</summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>The processed export files.</summary>
    public DbSet<ImportRun> ImportRuns => Set<ImportRun>();

    /// <summary>The raw rows kept from those files.</summary>
    public DbSet<RawRow> RawRows => Set<RawRow>();

    /// <summary>The account balances each import run's export reported.</summary>
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

    /// <summary>The shared password, as a hash. At most one row.</summary>
    public DbSet<Credential> Credentials => Set<Credential>();

    /// <inheritdoc />
    /// <remarks>
    /// The search functions are added here rather than by each host, so every
    /// context — the application's, a migration's, a test's — can run a search.
    /// </remarks>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder.AddInterceptors(new SearchFunctionsInterceptor());
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashPrismDbContext).Assembly);

        modelBuilder
            .HasDbFunction(typeof(SearchFunctions).GetMethod(nameof(SearchFunctions.Fold))!)
            .HasName(SearchFunctions.FoldName);
    }
}
