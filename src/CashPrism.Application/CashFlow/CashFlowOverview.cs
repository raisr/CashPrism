namespace CashPrism.Application.CashFlow;

/// <summary>
/// What the overview shows: income and spending per month up to the month the
/// stored data is current to, and where the money of the last complete month
/// went.
/// </summary>
/// <param name="CurrentTo">The date the stored data is current to.</param>
/// <param name="Months">
/// <see cref="CashFlowOverviewReader.MonthsShown"/> months, oldest first, a
/// month without bookings as zero. The last one is the running month.
/// </param>
/// <param name="SpendingByCategory">
/// Spending per main category in the last complete month, largest first.
/// </param>
public sealed record CashFlowOverview(
    DateOnly CurrentTo,
    IReadOnlyList<MonthlyCashFlow> Months,
    IReadOnlyList<CategorySpending> SpendingByCategory)
{
    /// <summary>
    /// The month <see cref="CurrentTo"/> falls in. Incomplete: the export was
    /// taken before it ended.
    /// </summary>
    public MonthlyCashFlow RunningMonth => Months[^1];

    /// <summary>The month before the running one, the latest the export covers in full.</summary>
    public MonthlyCashFlow LastCompleteMonth => Months[^2];

    /// <summary>The month before <see cref="LastCompleteMonth"/>, which it is compared with.</summary>
    public MonthlyCashFlow MonthBefore => Months[^3];

    /// <summary>
    /// How income changed against the month before, as a fraction — 0.02 for
    /// two per cent more. <see langword="null"/> when the month before had none.
    /// </summary>
    public decimal? IncomeChange => RelativeChange(LastCompleteMonth.IncomeInCents, MonthBefore.IncomeInCents);

    /// <summary>
    /// How spending changed against the month before, as a fraction.
    /// <see langword="null"/> when the month before had none.
    /// </summary>
    public decimal? SpendingChange => RelativeChange(LastCompleteMonth.SpendingInCents, MonthBefore.SpendingInCents);

    /// <summary>
    /// How much more was left than in the month before, in whole cents. An
    /// amount rather than a fraction: what was left may be small or negative,
    /// and a percentage of that says nothing.
    /// </summary>
    public long LeftChangeInCents => LastCompleteMonth.LeftInCents - MonthBefore.LeftInCents;

    private static decimal? RelativeChange(long current, long previous)
        => previous == 0 ? null : (decimal)(current - previous) / previous;
}
