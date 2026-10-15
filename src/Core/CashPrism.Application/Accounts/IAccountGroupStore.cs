using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Keeps the account groups and which account is in which.
/// </summary>
public interface IAccountGroupStore
{
    /// <summary>
    /// The stored grouping, or <c>null</c> while no group is stored — a fresh
    /// database, or one whose groups were deleted with the data.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<AccountGrouping?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces what is stored with <paramref name="grouping"/>, all or nothing.
    /// </summary>
    /// <param name="grouping">The grouping, hidden accounts included.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task SaveAsync(AccountGrouping grouping, CancellationToken cancellationToken = default);
}
