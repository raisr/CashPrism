namespace CashPrism.Application.Contracts;

/// <summary>
/// The contracts as the contracts page shows them.
/// </summary>
/// <param name="CurrentTo">The date the export is current to, which <see cref="PresumablyEnded"/> is measured from.</param>
/// <param name="MonthlyCostInCents">
/// What the spending contracts cost in an average month, as a positive number
/// in whole cents: each spread over a month by its interval. Income, transfers,
/// presumably ended contracts and contracts of an unknown interval are left out.
/// </param>
/// <param name="ByInterval">The spending contracts, one group per interval, the shortest interval first.</param>
/// <param name="Income">The contracts that bring money in.</param>
/// <param name="Transfers">The contracts that move money between two of the owner's own accounts.</param>
/// <param name="PresumablyEnded">The contracts whose bookings have stopped, whatever kind they are.</param>
public sealed record ContractOverview(
    DateOnly CurrentTo,
    long MonthlyCostInCents,
    IReadOnlyList<ContractGroup> ByInterval,
    ContractGroup Income,
    ContractGroup Transfers,
    ContractGroup PresumablyEnded);
