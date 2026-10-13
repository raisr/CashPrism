using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;

namespace CashPrism.TestSupport.Contracts;

/// <summary>
/// Keeps dismissals in memory, one per contract as the database store does.
/// </summary>
public sealed class FakeDismissedOverdueStore(params DismissedOverdue[] dismissals) : IDismissedOverdueStore
{
    private readonly List<DismissedOverdue> stored = [.. dismissals];

    /// <summary>What is stored, in the order it was dismissed.</summary>
    public IReadOnlyList<DismissedOverdue> Stored => stored;

    /// <inheritdoc />
    public Task<IReadOnlyList<DismissedOverdue>> ReadAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DismissedOverdue>>([.. stored]);

    /// <inheritdoc />
    public Task DismissAsync(DismissedOverdue dismissal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dismissal);

        stored.RemoveAll(existing => existing.ContractId == dismissal.ContractId);
        stored.Add(dismissal);

        return Task.CompletedTask;
    }
}
