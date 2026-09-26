using System.Globalization;

namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// Reads the export date out of a FinanzGuru worksheet name. The name is shaped
/// <c>YYYYMMDD_Export_Alle_Buchungen</c> and therefore changes with every
/// export, which is why a reader takes the worksheet by position — and why the
/// name is worth parsing at all: it is the only place the export says when it
/// was taken.
/// </summary>
/// <remarks>
/// The date decides which version of a booking wins on a re-import, so a name
/// that cannot be read is reported rather than guessed at. Guessing would let a
/// stale export silently overwrite a newer booking.
/// </remarks>
public static class FinanzguruSheetName
{
    /// <summary>The part of the sheet name that follows the date.</summary>
    public const string Suffix = "_Export_Alle_Buchungen";

    private const string DateFormat = "yyyyMMdd";

    /// <summary>
    /// Reads the export date out of <paramref name="sheetName"/>. Surrounding
    /// whitespace is ignored; everything else has to match exactly, because a
    /// name this reader does not recognise is a change to the export rather than
    /// something to work around.
    /// </summary>
    /// <param name="sheetName">The worksheet name, as the workbook carries it.</param>
    /// <param name="exportedOn">The date the export was taken, on success.</param>
    /// <returns>Whether the name could be read.</returns>
    public static bool TryParseExportDate(string? sheetName, out DateOnly exportedOn)
    {
        exportedOn = default;

        var name = sheetName?.Trim();

        if (string.IsNullOrEmpty(name) || !name.EndsWith(Suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var date = name[..^Suffix.Length];

        return DateOnly.TryParseExact(
            date,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out exportedOn);
    }
}
