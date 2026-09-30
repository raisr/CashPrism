using System.Globalization;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// The four <c>Analyse-</c> period columns, derived from a booking date the way
/// <c>docs/finanzguru-export.md</c> measured them. They carry nothing the date
/// does not, so a generated date takes generated labels with it — otherwise the
/// week of a row would still say when it really happened.
/// </summary>
public static class PeriodLabels
{
    /// <summary>
    /// <c>Analyse-Woche</c>, shaped <c>YYYY-WW</c>: week 1 runs from 1 January to
    /// the first Saturday, and a new week starts every Sunday.
    /// </summary>
    public static string Week(DateOnly date)
    {
        var firstOfYear = new DateOnly(date.Year, 1, 1);
        var daysSinceTheSundayBefore = (date.DayOfYear - 1) + (int)firstOfYear.DayOfWeek;

        return $"{date.Year:0000}-{(daysSinceTheSundayBefore / 7) + 1:00}";
    }

    /// <summary><c>Analyse-Monat</c>, shaped <c>YYYY-MM</c>.</summary>
    public static string Month(DateOnly date) => $"{date.Year:0000}-{date.Month:00}";

    /// <summary><c>Analyse-Quartal</c>, shaped <c>YYYY-Qn</c>.</summary>
    public static string Quarter(DateOnly date) => $"{date.Year:0000}-Q{((date.Month - 1) / 3) + 1}";

    /// <summary>
    /// <c>Analyse-Jahr</c> as the numeric cell's text. The export writes the year
    /// as a bare number with a trailing <c>.0</c>, the way Java prints a double.
    /// </summary>
    public static string Year(DateOnly date) => date.Year.ToString(CultureInfo.InvariantCulture) + ".0";
}
