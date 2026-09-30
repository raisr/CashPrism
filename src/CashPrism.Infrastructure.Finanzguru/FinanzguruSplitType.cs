using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Reads the <c>Split-Typ</c> column of a FinanzGuru export. The export names the
/// roles in German, so something has to translate — and it is this layer, because
/// a German word in a spreadsheet is a property of the export, not of a booking.
/// </summary>
/// <remarks>
/// Strict for the same reason as <see cref="FinanzguruFlag"/>: a role we cannot
/// read decides whether an amount is counted once or twice, and guessing
/// <see cref="SplitRole.None"/> would turn an unreadable cell into a silently
/// wrong total. Only one split booking occurred in the measured exports, so the
/// three words are known and their edge cases are not — see
/// <c>docs/finanzguru-export.md</c>.
/// </remarks>
public static class FinanzguruSplitType
{
    /// <summary>The word the export writes for the booking that was split.</summary>
    public const string Original = "Original";

    /// <summary>The word the export writes for a part the original was split into.</summary>
    public const string Part = "Teilbuchung";

    /// <summary>The word the export writes for what was left over after the parts.</summary>
    public const string Remainder = "Restbetrag";

    /// <summary>
    /// Converts one cell of the <c>Split-Typ</c> column to a <see cref="SplitRole"/>.
    /// An empty cell is the ordinary case and reads as <see cref="SplitRole.None"/>;
    /// surrounding whitespace is ignored and the comparison is case-insensitive.
    /// </summary>
    /// <param name="value">The cell's value, as the export stores it.</param>
    /// <param name="row">The one-based worksheet row the cell sits in, for the failure.</param>
    /// <returns>A successful result carrying the role, or a failure naming the row.</returns>
    public static FinanzguruSplitTypeResult Parse(string? value, int row)
    {
        var word = value?.Trim();

        if (string.IsNullOrEmpty(word))
        {
            return FinanzguruSplitTypeResult.Success(SplitRole.None);
        }

        if (string.Equals(word, Original, StringComparison.OrdinalIgnoreCase))
        {
            return FinanzguruSplitTypeResult.Success(SplitRole.Original);
        }

        if (string.Equals(word, Part, StringComparison.OrdinalIgnoreCase))
        {
            return FinanzguruSplitTypeResult.Success(SplitRole.Part);
        }

        if (string.Equals(word, Remainder, StringComparison.OrdinalIgnoreCase))
        {
            return FinanzguruSplitTypeResult.Success(SplitRole.Remainder);
        }

        return FinanzguruSplitTypeResult.Failure(
            ImportError.UnknownSplitType(FinanzguruColumns.SplitType, row, word, [Original, Part, Remainder]));
    }
}
