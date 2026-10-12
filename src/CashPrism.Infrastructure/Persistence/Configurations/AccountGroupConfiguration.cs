using CashPrism.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AccountGroup"/> onto its table.
/// </summary>
public sealed class AccountGroupConfiguration : IEntityTypeConfiguration<AccountGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AccountGroup> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AccountGroups");

        builder.HasKey(group => group.Id);
        builder.Property(group => group.Id).ValueGeneratedNever();

        // Declared rather than left to convention: a get-only property is not picked
        // up automatically, and the constructor can only be bound to mapped ones.
        builder.Property(group => group.Name);
        builder.Property(group => group.Position);
    }
}
