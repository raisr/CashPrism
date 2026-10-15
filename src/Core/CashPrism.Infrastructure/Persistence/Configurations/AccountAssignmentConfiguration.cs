using CashPrism.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AccountAssignment"/> onto its table.
/// </summary>
public sealed class AccountAssignmentConfiguration : IEntityTypeConfiguration<AccountAssignment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AccountAssignment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AccountAssignments");

        // An account is in one group at a time, so the account alone is the key.
        // No foreign key to the bookings or balances: the assignment outlives them.
        builder.HasKey(assignment => assignment.AccountReference);

        // Declared rather than left to convention: a get-only property is not picked
        // up automatically, and the constructor can only be bound to mapped ones.
        builder.Property(assignment => assignment.GroupId);
        builder.Property(assignment => assignment.Position);

        // The group owns its assignments: deleting it takes them with it.
        builder.HasOne<AccountGroup>()
            .WithMany()
            .HasForeignKey(assignment => assignment.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
