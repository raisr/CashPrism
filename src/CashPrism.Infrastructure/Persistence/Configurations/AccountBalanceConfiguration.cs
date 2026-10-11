using CashPrism.Domain.Accounts;
using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AccountBalance"/> onto its table.
/// </summary>
public sealed class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AccountBalances");

        // One export reports one balance per account, so the run and the account
        // are the key — the same reasoning as the raw rows.
        builder.HasKey(balance => new { balance.ImportRunId, balance.AccountReference });

        // Declared rather than left to convention: a get-only property is not picked
        // up automatically, and the constructor can only be bound to mapped ones.
        builder.Property(balance => balance.AccountName);
        builder.Property(balance => balance.BalanceInCents);
        builder.Property(balance => balance.Currency);
        builder.Property(balance => balance.LatestBookedOn);

        // The run owns its balances: deleting it takes them with it.
        builder.HasOne<ImportRun>()
            .WithMany()
            .HasForeignKey(balance => balance.ImportRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
