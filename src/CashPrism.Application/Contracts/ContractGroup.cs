using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// Contracts the contracts page lists together, with what they add up to.
/// </summary>
/// <param name="Interval">
/// The interval every contract here recurs at, as Finanzguru names it, or
/// <see langword="null"/> for a section that mixes intervals.
/// </param>
/// <param name="Contracts">The contracts, the largest amount first.</param>
public sealed record ContractGroup(string? Interval, IReadOnlyList<Contract> Contracts)
{
    /// <summary>How many contracts the group holds.</summary>
    public int Count => Contracts.Count;

    /// <summary>The signed sum of their current amounts, in whole cents.</summary>
    public long SumInCents => Contracts.Sum(contract => contract.AmountInCents);
}
