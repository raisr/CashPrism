using System.Globalization;
using CashPrism.Web.Bookings;

namespace CashPrism.Web.Overview;

/// <summary>
/// How the overview writes its figures. A summary is not a booking: it carries
/// no plus sign, and where the design rounds — the lead, the chart, the
/// categories — it rounds to whole euros (<c>design/README.md</c>, numbers).
/// </summary>
/// <remarks>
/// Every amount here is in euros. The sums add up bookings of every account,
/// and the scale CashPrism keeps money in already assumes the two-decimal
/// currency a Finanzguru export carries.
/// </remarks>
public static class OverviewFormat
{
    private const string Euro = "EUR";

    /// <summary>The real minus sign, U+2212, as the booking list writes it.</summary>
    private const char Minus = '−';

    /// <summary>Keeps a figure and its unit on one line.</summary>
    private const char NoBreakSpace = ' ';

    /// <summary>
    /// A sum to the cent, cut where the cents begin, for a tile that fades
    /// them: no plus in front, a real minus where more went out than came in.
    /// </summary>
    /// <param name="amountInCents">The sum in whole cents.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static (string Whole, string Cents) SumParts(long amountInCents, IFormatProvider? formatProvider = null)
    {
        // The booking format is the one that knows separators, the minus and
        // the currency; a sum only does without its plus.
        var (whole, cents) = BookingFormat.AmountParts(amountInCents, Euro, formatProvider);

        return (whole.TrimStart('+'), cents);
    }

    /// <summary>A sum rounded to whole euros: <c>1.439 €</c>, <c>−312 €</c>.</summary>
    /// <param name="amountInCents">The sum in whole cents.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string WholeEuros(long amountInCents, IFormatProvider? formatProvider = null)
    {
        var euros = Math.Round(amountInCents / 100m, MidpointRounding.AwayFromZero);
        var figure = Math.Abs(euros).ToString("#,##0", formatProvider ?? CultureInfo.CurrentCulture);

        return (euros < 0 ? Minus.ToString() : string.Empty) + figure + NoBreakSpace + "€";
    }

    /// <summary>
    /// A difference in whole euros, always signed, because it says which way
    /// something moved: <c>+312 €</c>, <c>−45 €</c>, <c>0 €</c>.
    /// </summary>
    /// <param name="amountInCents">The difference in whole cents.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string SignedWholeEuros(long amountInCents, IFormatProvider? formatProvider = null)
    {
        var text = WholeEuros(amountInCents, formatProvider);

        return Math.Round(amountInCents / 100m, MidpointRounding.AwayFromZero) > 0 ? "+" + text : text;
    }

    /// <summary>
    /// A change as whole per cent, signed, with the space the design puts
    /// before the sign: <c>+2 %</c>, <c>−29 %</c>.
    /// </summary>
    /// <param name="change">The change as a fraction — 0.02 for two per cent.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string Percent(decimal change, IFormatProvider? formatProvider = null)
    {
        var percent = Math.Round(change * 100m, MidpointRounding.AwayFromZero);
        var figure = Math.Abs(percent).ToString("0", formatProvider ?? CultureInfo.CurrentCulture);
        var sign = percent switch
        {
            > 0 => "+",
            < 0 => Minus.ToString(),
            _ => string.Empty,
        };

        return sign + figure + NoBreakSpace + "%";
    }

    /// <summary>
    /// A month as the chart's axis names it: <c>Okt 26</c>. The culture's
    /// abbreviation loses its full stop, which on a crowded axis is noise.
    /// </summary>
    /// <param name="month">Any day of the month.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string MonthShort(DateOnly month, IFormatProvider? formatProvider = null)
    {
        var provider = formatProvider ?? CultureInfo.CurrentCulture;
        var name = DateTimeFormatInfo.GetInstance(provider).GetAbbreviatedMonthName(month.Month).TrimEnd('.');

        return name + " " + month.ToString("yy", provider);
    }

    /// <summary>A month written out: <c>September 2026</c>.</summary>
    /// <param name="month">Any day of the month.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string MonthLong(DateOnly month, IFormatProvider? formatProvider = null)
        => month.ToString("MMMM yyyy", formatProvider ?? CultureInfo.CurrentCulture);

    /// <summary>A month's name alone: <c>September</c>.</summary>
    /// <param name="month">Any day of the month.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string MonthName(DateOnly month, IFormatProvider? formatProvider = null)
        => month.ToString("MMMM", formatProvider ?? CultureInfo.CurrentCulture);
}
