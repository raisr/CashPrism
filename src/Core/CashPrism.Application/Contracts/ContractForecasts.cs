namespace CashPrism.Application.Contracts;

/// <summary>
/// Every contract with its forecast, and the date they are measured from.
/// </summary>
/// <param name="CurrentTo">The date the export is current to.</param>
/// <param name="Contracts">Every contract, in no particular order.</param>
public sealed record ContractForecasts(DateOnly CurrentTo, IReadOnlyList<ContractForecast> Contracts);
