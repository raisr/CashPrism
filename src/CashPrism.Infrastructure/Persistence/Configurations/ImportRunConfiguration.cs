using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="ImportRun"/> onto its table.
/// </summary>
public sealed class ImportRunConfiguration : IEntityTypeConfiguration<ImportRun>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ImportRun> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ImportRuns");

        builder.HasKey(run => run.Id);

        // Declared rather than left to convention: a get-only property is not picked
        // up automatically, and the constructor can only be bound to mapped ones.
        builder.Property(run => run.FileName);
        builder.Property(run => run.FileHash);
        builder.Property(run => run.SheetName);
        builder.Property(run => run.ExportedOn);
        // Stored as a UTC DateTime rather than as the DateTimeOffset the model
        // carries. SQLite has no type that orders a DateTimeOffset — it writes
        // one as text with the offset appended, which only compares correctly
        // while every row carries the same offset, so EF Core refuses to
        // translate an ORDER BY over it at all. Without this the list of runs
        // could not ask the database for its own order, and nothing could ever
        // ask for the runs since a date.
        //
        // Nothing is lost by dropping the offset: the only clock that writes
        // this is UTC, and what comes back says so.
        builder.Property(run => run.ImportedAt)
            .HasConversion(
                run => run.UtcDateTime,
                stored => new DateTimeOffset(stored, TimeSpan.Zero));
        builder.Property(run => run.RowsRead);
        builder.Property(run => run.BookingsInserted);
        builder.Property(run => run.BookingsUpdated);
        builder.Property(run => run.BookingsUnchanged);
        builder.Property(run => run.IsComplete);

        // The hash exists to recognise a file that has been imported before, so it
        // is indexed. Not unique: importing the same file twice is something a
        // person may do, and what happens then is the import's decision, not a
        // constraint violation.
        builder.HasIndex(run => run.FileHash);
    }
}
