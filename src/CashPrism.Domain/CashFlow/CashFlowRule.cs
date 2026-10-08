using System.Linq.Expressions;
using CashPrism.Domain.Bookings;

namespace CashPrism.Domain.CashFlow;

/// <summary>
/// Which bookings count as income or spending, and how much each contributes.
/// The one rule every analysis uses, so two screens never disagree about what
/// a month cost.
/// </summary>
/// <remarks>
/// <para>
/// Expressions rather than methods, so the rule runs where the bookings are:
/// the sums are taken by the database over whole cents, and a method could
/// only run after every booking had been loaded.
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
    public static Expression<Func<Booking, bool>> Counts { get; } =
        booking => !booking.IsTransfer && booking.SplitRole != SplitRole.Original;

    /// <summary>
    /// What a counted booking contributes. The sign decides: a positive amount
    /// is income, a negative one spending.
    /// </summary>
    public static Expression<Func<Booking, CashFlowEntry>> ToEntry { get; } =
        booking => new CashFlowEntry
        {
            BookedOn = booking.BookedOn,
            Category = booking.Category,
            IncomeInCents = booking.AmountInCents > 0 ? booking.AmountInCents : 0,
            SpendingInCents = booking.AmountInCents < 0 ? -booking.AmountInCents : 0,
        };
}
