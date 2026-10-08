using CashPrism.Application.Access;
using CashPrism.Domain.Access;
using CashPrism.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Keeps the credential in the one row of its table.
/// </summary>
public sealed class CredentialStore : ICredentialStore
{
    private readonly CashPrismDbContext context;

    /// <summary>
    /// Creates the store.
    /// </summary>
    /// <param name="context">The database.</param>
    public CredentialStore(CashPrismDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        this.context = context;
    }

    /// <inheritdoc />
    public async Task<Credential?> GetAsync(CancellationToken cancellationToken = default)
    {
        // Untracked: the question is asked on every login and every redirect,
        // and nothing read here is written back.
        return await context.Credentials.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Credential credential, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credential);

        context.Credentials.Add(credential);
        context.Entry(credential).Property(CredentialConfiguration.KeyProperty).CurrentValue =
            CredentialConfiguration.TheOnlyKey;

        await context.SaveChangesAsync(cancellationToken);
    }
}
