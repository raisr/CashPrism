namespace CashPrism.Application.Contracts;

/// <summary>
/// The contract bookings expected soon, as the overview lists them.
/// </summary>
/// <param name="CurrentTo">The date the export is current to, which the window is measured from.</param>
/// <param name="Contracts">The contracts, overdue ones first, the longest overdue on top; the rest by expected date.</param>
public sealed record UpcomingContracts(DateOnly CurrentTo, IReadOnlyList<ContractForecast> Contracts);
