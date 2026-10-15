namespace CashPrism.Application.Bookings;

/// <summary>
/// The figures over every booking a filter lists, not only over the page on
/// screen. Which bookings are summed is <see cref="Domain.CashFlow.CashFlowRule.SumsInList"/>.
/// </summary>
/// <param name="Count">How many bookings the filter lists, the original of a split booking included.</param>
/// <param name="IncomeInCents">The positive amounts added up, in whole cents.</param>
/// <param name="SpendingInCents">The negative amounts added up, as a positive number of whole cents.</param>
public sealed record BookingSummary(int Count, long IncomeInCents, long SpendingInCents)
{
    /// <summary>What came in minus what went out, in whole cents.</summary>
    public long BalanceInCents => IncomeInCents - SpendingInCents;
}
