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
        builder.Property(run => run.ImportedAt);

        // The hash exists to recognise a file that has been imported before, so it
        // is indexed. Not unique: importing the same file twice is something a
        // person may do, and what happens then is the import's decision, not a
        // constraint violation.
        builder.HasIndex(run => run.FileHash);
    }
}
