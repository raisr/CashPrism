using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Reads the account balances the stored exports reported.
/// </summary>
public interface IAccountBalanceReader
{
    /// <summary>
    /// The balances of the latest completed import run, in no particular order,
    /// or none when nothing was imported. "Latest" is decided the way a booking's
    /// state is — by <see cref="Domain.Imports.ImportRun.IsLaterThan"/> — so the
    /// balances never describe an older export than the bookings beside them.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<AccountBalance>> ReadLatestAsync(CancellationToken cancellationToken = default);
}
