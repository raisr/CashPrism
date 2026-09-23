namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Reads the yes/no columns of a FinanzGuru export. The export writes them as
/// German words rather than as booleans, so something has to translate — and it
/// is this layer, because a German word in a spreadsheet is a property of the
/// export, not of a booking.
/// </summary>
/// <remarks>
/// The conversion is strict on purpose. A column that FinanzGuru starts filling
/// with a third word is a change we want to be told about, and defaulting an
/// unreadable value to <c>false</c> would instead turn it into a silently wrong
/// total: <c>Analyse-Umbuchung</c> alone decides for 916 of 6,327 measured rows
/// whether they are counted as income and spending at all.
/// </remarks>
public static class FinanzguruFlag
{
    /// <summary>The word the export writes for yes.</summary>
    public const string Yes = "ja";

    /// <summary>The word the export writes for no.</summary>
    public const string No = "nein";

    /// <summary>
    /// Converts one cell of a yes/no column to a <see cref="bool"/>. Surrounding
    /// whitespace is ignored and the comparison is case-insensitive; anything
    /// that is still neither word fails and says where.
    /// </summary>
    /// <param name="value">The cell's value, as the export stores it.</param>
    /// <param name="column">The header name of the column, for the failure message.</param>
    /// <param name="row">The one-based worksheet row the cell sits in, for the failure message.</param>
    /// <returns>
    /// A successful result carrying the value, or a failure naming the column and
    /// the row. See <see cref="FinanzguruFlagResult"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="column"/> is missing.</exception>
    public static FinanzguruFlagResult Parse(string? value, string column, int row)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);

        var word = value?.Trim();

        if (string.Equals(word, Yes, StringComparison.OrdinalIgnoreCase))
        {
            return FinanzguruFlagResult.Success(true);
        }

        if (string.Equals(word, No, StringComparison.OrdinalIgnoreCase))
        {
            return FinanzguruFlagResult.Success(false);
        }

        var what = string.IsNullOrEmpty(word) ? "is empty" : $"carries '{word}'";

        return FinanzguruFlagResult.Failure(
            $"Column '{column}' in row {row} {what}; expected '{Yes}' or '{No}'.");
    }
}
