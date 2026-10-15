using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Reads what every stored booking contributes to its account's balance
/// history — a narrow projection, not the bookings themselves.
/// </summary>
public interface IBalanceMovementReader
{
    /// <summary>The movements of every account, in no particular order.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    Task<IReadOnlyList<BalanceMovement>> ReadAllAsync(CancellationToken cancellationToken = default);
}
