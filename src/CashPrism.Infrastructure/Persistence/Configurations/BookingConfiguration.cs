using CashPrism.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Booking"/> onto its table. The model has no setters and no
/// parameterless constructor, so EF Core binds through the constructor — its
/// parameter names match the properties, which is the whole contract.
/// </summary>
/// <remarks>
/// Every property is declared here rather than left to convention. Convention does
/// not pick up a get-only property, and an unmapped property is one the constructor
/// cannot be bound to, which fails when the model is built rather than when a row
/// is read.
/// </remarks>
public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Bookings");

        // The fingerprint is the identity of a booking, so it is the key rather
        // than a surrogate id with a unique index beside it. That also makes the
        // re-import an upsert against the primary key.
        builder.HasKey(booking => booking.Fingerprint);

        builder.Property(booking => booking.BookedOn);
        builder.Property(booking => booking.AmountInCents);
        builder.Property(booking => booking.Currency);
        builder.Property(booking => booking.AccountReference);
        builder.Property(booking => booking.AccountName);
        builder.Property(booking => booking.Counterparty);
        builder.Property(booking => booking.CounterpartyAccount);
        builder.Property(booking => booking.PaymentReference);
        builder.Property(booking => booking.Category);
        builder.Property(booking => booking.SubCategory);
        builder.Property(booking => booking.IsTransfer);

        // Stored as its name, not its number: a database someone opens in a SQLite
        // browser should be readable, and the numeric values of the enum are not a
        // contract anybody promised to keep.
        builder.Property(booking => booking.SplitRole)
            .HasConversion<string>();

        // No foreign key to the booking a split part points at: the original may
        // sit in an export we have not imported, and a constraint would then reject
        // a row the export legitimately contains.
        builder.Property(booking => booking.OriginalFingerprint);

        builder.Ignore(booking => booking.IsSplitPart);
    }
}
