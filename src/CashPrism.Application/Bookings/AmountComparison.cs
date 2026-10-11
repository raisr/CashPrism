namespace CashPrism.Application.Bookings;

/// <summary>How an amount in a search compares with a booking's amount.</summary>
public enum AmountComparison
{
    /// <summary><c>&gt;</c>: more than the bound.</summary>
    GreaterThan = 1,

    /// <summary><c>&gt;=</c>: the bound or more.</summary>
    GreaterThanOrEqual = 2,

    /// <summary><c>&lt;</c>: less than the bound.</summary>
    LessThan = 3,

    /// <summary><c>&lt;=</c>: the bound or less.</summary>
    LessThanOrEqual = 4,

    /// <summary><c>=</c>: exactly the bound.</summary>
    Equal = 5,
}
