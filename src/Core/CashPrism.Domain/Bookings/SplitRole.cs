namespace CashPrism.Domain.Bookings;

/// <summary>
/// The role a booking plays in a split booking. A split appears in the export as
/// several rows: one original and the parts it was split into, and the parts'
/// amounts add up to the original amount. Anything that sums amounts has to know
/// which of the rows in front of it are parts, or it counts the booking twice.
/// </summary>
/// <remarks>
/// The export names these roles in German, in its <c>Split-Typ</c> column; the
/// translation belongs to the reader of that export, not here. Only one split
/// booking occurred in the measured exports, so the mechanism is known and its
/// edge cases are not — see <c>docs/finanzguru-export.md</c>.
/// </remarks>
public enum SplitRole
{
    /// <summary>The booking is not part of a split. The ordinary case.</summary>
    None = 0,

    /// <summary>The booking that was split. Its amount is the sum of the parts.</summary>
    Original = 1,

    /// <summary>A part the original was split into.</summary>
    Part = 2,

    /// <summary>The part of the original that was left over after the other parts.</summary>
    Remainder = 3,
}
