using CashPrism.Domain.Bookings;

namespace CashPrism.Domain.Accounts;

/// <summary>
/// What a booking contributes to its account's balance history: when, how much,
/// and the balance the export reports beside it.
/// </summary>
/// <param name="AccountReference">The account the booking belongs to.</param>
/// <param name="BookedOn">The day the booking was posted on.</param>
/// <param name="AmountInCents">The signed amount in whole cents.</param>
/// <param name="SplitRole">The booking's role in a split, if any.</param>
/// <param name="ReportedBalanceInCents">The <c>Kontostand</c> the export carries on the booking's row.</param>
public sealed record BalanceMovement(
    string AccountReference,
    DateOnly BookedOn,
    long AmountInCents,
    SplitRole SplitRole,
    long ReportedBalanceInCents);
