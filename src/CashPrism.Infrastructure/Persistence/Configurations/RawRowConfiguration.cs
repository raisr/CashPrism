using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="RawRow"/> onto its table.
/// </summary>
public sealed class RawRowConfiguration : IEntityTypeConfiguration<RawRow>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RawRow> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RawRows");

        // One run holds at most one state of a booking, so the run and the
        // fingerprint are the key. A surrogate id would add a column and an index
        // without describing anything the pair does not already say.
        builder.HasKey(row => new { row.ImportRunId, row.Fingerprint });

        // Declared rather than left to convention, for the same reason as the other
        // two: convention does not map a get-only property.
        builder.Property(row => row.Json);

        // The run owns its rows: deleting it takes them with it. Declared without a
        // navigation property, because the models stay plain — the relationship
        // lives here, not in the domain.
        builder.HasOne<ImportRun>()
            .WithMany()
            .HasForeignKey(row => row.ImportRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
