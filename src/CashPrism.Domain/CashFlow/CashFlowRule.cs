using System.Linq.Expressions;
using CashPrism.Domain.Bookings;

namespace CashPrism.Domain.CashFlow;

/// <summary>
/// Which bookings count, and how a month's income and spending follow from
/// them. The one rule every analysis uses, so two screens never disagree
/// about what a month cost — and the rule Finanzguru's own analysis applies,
/// so CashPrism shows the figures the app shows.
/// </summary>
/// <remarks>
/// <para>
/// A month is netted per main category first: the counted bookings of each
/// category are added up, signs and all. A category that comes out positive
/// is income, one that comes out negative is spending. A refund therefore
/// makes its category's spending smaller instead of counting as income, and
/// a category that brought in more than it cost in a month counts as income
/// for that month. The measurement this rests on is in
/// <c>docs/finanzguru-export.md</c>.
/// </para>
/// <para>
/// The flag that leaves a booking out of the disposable income is ignored on
/// purpose. It answers what is left to spend freely, not what was spent.
/// </para>
/// </remarks>
public static class CashFlowRule
{
    /// <summary>
    /// Whether a booking counts at all. A transfer between two of the owner's
    /// own accounts is neither income nor spending, and of a split booking the
    /// parts count while the original does not — it is the sum of the parts,
    /// and counting it as well would count the money twice.
    /// </summary>
    /// <remarks>
    /// An expression rather than a method, so the rule runs where the bookings
    /// are: the category sums are taken by the database over whole cents.
    /// </remarks>
    public static Expression<Func<Booking, bool>> Counts { get; } =
        booking => !booking.IsTransfer && booking.SplitRole != SplitRole.Original;

    /// <summary>A month's income: the sum of the categories that came out positive.</summary>
    /// <param name="categoryNetsInCents">What each main category added up to in the month, in whole cents.</param>
    public static long IncomeInCents(IEnumerable<long> categoryNetsInCents)
    {
        ArgumentNullException.ThrowIfNull(categoryNetsInCents);

        return categoryNetsInCents.Where(net => net > 0).Sum();
    }

    /// <summary>
    /// A month's spending, as a positive number: the sum of the categories that
    /// came out negative.
    /// </summary>
    /// <param name="categoryNetsInCents">What each main category added up to in the month, in whole cents.</param>
    public static long SpendingInCents(IEnumerable<long> categoryNetsInCents)
    {
        ArgumentNullException.ThrowIfNull(categoryNetsInCents);

        return -categoryNetsInCents.Where(net => net < 0).Sum();
    }

    /// <summary>
    /// What one main category cost in a month, as a positive number: its sum
    /// when that came out negative, nothing when it brought in as much as it
    /// cost or more.
    /// </summary>
    /// <param name="categoryNetInCents">What the category added up to in the month, in whole cents.</param>
    public static long CategorySpendingInCents(long categoryNetInCents)
        => categoryNetInCents < 0 ? -categoryNetInCents : 0;
}
