using CashPrism.Domain.Access;

namespace CashPrism.Application.Access;

/// <summary>
/// Keeps the one <see cref="Credential"/> CashPrism has.
/// </summary>
public interface ICredentialStore
{
    /// <summary>The stored credential, or <c>null</c> while no password is set.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<Credential?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Stores the first credential.</summary>
    /// <param name="credential">The credential to store.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task AddAsync(Credential credential, CancellationToken cancellationToken = default);
}
