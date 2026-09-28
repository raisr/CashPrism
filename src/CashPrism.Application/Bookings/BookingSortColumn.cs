namespace CashPrism.Application.Bookings;

/// <summary>
/// A column the booking list can be ordered by. An enum rather than a column
/// name: the set of orderings is closed, and nothing a browser sends ever
/// reaches a query as text.
/// </summary>
public enum BookingSortColumn
{
    /// <summary>The date the booking was posted. The default order.</summary>
    BookedOn = 0,

    /// <summary>The name of the account the booking belongs to.</summary>
    AccountName = 1,

    /// <summary>The other party of the booking.</summary>
    Counterparty = 2,

    /// <summary>The payment reference.</summary>
    PaymentReference = 3,

    /// <summary>FinanzGuru's top-level category.</summary>
    Category = 4,

    /// <summary>The signed amount.</summary>
    AmountInCents = 5,
}
