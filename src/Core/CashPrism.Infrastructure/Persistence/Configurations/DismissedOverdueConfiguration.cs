using CashPrism.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="DismissedOverdue"/> onto its table.
/// </summary>
public sealed class DismissedOverdueConfiguration : IEntityTypeConfiguration<DismissedOverdue>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DismissedOverdue> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("DismissedOverdues");

        // A marker is dismissed for one date of one contract. No foreign key to
        // the bookings: a contract has no table of its own, only its identifier.
        builder.HasKey(dismissal => new { dismissal.ContractId, dismissal.ExpectedOn });
    }
}
