using CashPrism.Domain.Contracts;

namespace CashPrism.Application.Contracts;

/// <summary>
/// A contract with what is expected of it next.
/// </summary>
/// <param name="Contract">The contract, as its newest booking describes it.</param>
/// <param name="NextBookingOn">
/// The day its next booking is expected, or <see langword="null"/> where
/// nothing is expected — see <see cref="Contract.NextBookingOn"/>.
/// </param>
/// <param name="DaysOverdue">
/// How many days that booking is overdue, or <see langword="null"/> where it is
/// not, or where the person dismissed the marker for that day.
/// </param>
public sealed record ContractForecast(Contract Contract, DateOnly? NextBookingOn, int? DaysOverdue);
