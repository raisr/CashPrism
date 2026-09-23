using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// The database. One SQLite file on local disk, holding the bookings, the import
/// runs and the raw rows they came from.
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

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashPrismDbContext).Assembly);
    }
}
