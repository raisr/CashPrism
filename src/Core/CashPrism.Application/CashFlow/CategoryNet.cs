namespace CashPrism.Application.CashFlow;

/// <summary>
/// What the counted bookings of one main category added up to in one month,
/// signs and all, in whole cents.
/// </summary>
/// <param name="Month">The first day of the month.</param>
/// <param name="Category">Finanzguru's main category.</param>
/// <param name="NetInCents">The sum: positive when the category brought in more than it cost.</param>
public sealed record CategoryNet(DateOnly Month, string Category, long NetInCents);
