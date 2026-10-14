using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;
using CashPrism.Domain.Bookings;

namespace CashPrism.TestSupport.Accounts;

/// <summary>Hands out the movements a test added, as the database reader would.</summary>
public sealed class FakeBalanceMovementReader : IBalanceMovementReader
{
    /// <summary>The movements handed out.</summary>
    public List<BalanceMovement> Movements { get; } = [];

    /// <summary>Adds a plain booking's movement.</summary>
    public void Add(string accountReference, DateOnly bookedOn, long amountInCents, long reportedBalanceInCents)
        => Movements.Add(new BalanceMovement(accountReference, bookedOn, amountInCents, SplitRole.None, reportedBalanceInCents));

    /// <inheritdoc />
    public Task<IReadOnlyList<BalanceMovement>> ReadAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BalanceMovement>>(Movements);
}
