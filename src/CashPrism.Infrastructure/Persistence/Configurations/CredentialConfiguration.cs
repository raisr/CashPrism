using CashPrism.Domain.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashPrism.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="Credential"/> onto its table.
/// </summary>
public sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    /// <summary>
    /// The key of the one row the table may hold. A shadow property, because
    /// identity means nothing to the model: there is only ever one credential.
    /// </summary>
    public const string KeyProperty = "Id";

    /// <summary>The value <see cref="KeyProperty"/> always has.</summary>
    public const int TheOnlyKey = 1;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Credentials");

        // A fixed key rather than a generated one: a second credential then
        // fails as a duplicate key instead of quietly sitting beside the first.
        builder.Property<int>(KeyProperty).ValueGeneratedNever();
        builder.HasKey(KeyProperty);

        // Declared rather than left to convention: a get-only property is not picked
        // up automatically, and the constructor can only be bound to mapped ones.
        builder.Property(credential => credential.PasswordHash);
        builder.Property(credential => credential.Generation);

        // Stored as a UTC DateTime for the reason given on ImportRun.ImportedAt.
        builder.Property(credential => credential.SetAt)
            .HasConversion(
                setAt => setAt.UtcDateTime,
                stored => new DateTimeOffset(stored, TimeSpan.Zero));
    }
}
