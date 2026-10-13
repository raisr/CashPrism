using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;

namespace CashPrism.TestSupport.Contracts;

/// <summary>
/// Hands out a fixed date and fixed contracts, the way the database reader
/// would answer them.
/// </summary>
public sealed class FakeContractReader(DateOnly? currentTo, params Contract[] contracts) : IContractReader
{
    /// <inheritdoc />
    public Task<DateOnly?> ReadCurrentToAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(currentTo);

    /// <inheritdoc />
    public Task<IReadOnlyList<Contract>> ReadAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Contract>>(contracts);
}
