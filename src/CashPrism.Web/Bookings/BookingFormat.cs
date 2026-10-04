using System.Globalization;

namespace CashPrism.Web.Bookings;

/// <summary>
/// How a booking's figures are written into the list. The two decisions here —
/// that an amount always carries its sign, and that a booking date never shows
/// its time — are the ones a reader of the markup would otherwise have to infer
/// from a format string.
/// </summary>
public static class BookingFormat
{
    /// <summary>
    /// Always two decimals, without a sign of their own. The sign is what
    /// carries the meaning of a credit or a debit, so it is written in front by
    /// hand: .NET's minus is the hyphen-minus, and the design asks for the real
    /// one. A zero takes no sign, because nothing is neither.
    /// </summary>
    private const string TwoDecimals = "#,##0.00";

    /// <summary>The real minus sign, U+2212 — wider than a hyphen, and level with the plus.</summary>
    private const char Minus = '−';

    /// <summary>Keeps the figure and its currency on one line.</summary>
    private const char NoBreakSpace = ' ';

    /// <summary>
    /// Formats an amount for display, with its currency behind it. Dividing by a
    /// hundred happens here and nowhere else: whole cents are what the model, the
    /// database and every use case hold, and formatting for a person is the one
    /// place the number is allowed to become fractional (see <c>Agents.md</c>).
    /// </summary>
    /// <param name="amountInCents">The signed amount in whole cents.</param>
    /// <param name="currency">The ISO 4217 code the amount is in.</param>
    /// <param name="formatProvider">
    /// The culture to format in. Defaults to the current one, which the host
    /// pins to German — the UI never reads a culture of its own.
    /// </param>
    /// <returns>
    /// The amount with a plus for income, a real minus for spending and no sign
    /// for nothing, then a no-break space and the currency — see
    /// <see cref="CurrencyOf"/>.
    /// </returns>
    public static string Amount(long amountInCents, string currency, IFormatProvider? formatProvider = null)
    {
        var (whole, cents) = AmountParts(amountInCents, currency, formatProvider);

        return whole + cents;
    }

    /// <summary>
    /// The same text as <see cref="Amount"/>, cut where the cents begin — for a
    /// large amount that sets the cents apart from the euros.
    /// </summary>
    /// <param name="amountInCents">The signed amount in whole cents.</param>
    /// <param name="currency">The ISO 4217 code the amount is in.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    /// <returns>
    /// Everything before the decimal separator, and everything from it on: the
    /// separator, the cents and the currency.
    /// </returns>
    public static (string Whole, string Cents) AmountParts(
        long amountInCents,
        string currency,
        IFormatProvider? formatProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var provider = formatProvider ?? CultureInfo.CurrentCulture;
        var sign = amountInCents switch
        {
            > 0 => "+",
            < 0 => Minus.ToString(),
            _ => string.Empty,
        };

        var figure = (Math.Abs((decimal)amountInCents) / 100m).ToString(TwoDecimals, provider);
        var separator = NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator;
        var cut = figure.LastIndexOf(separator, StringComparison.Ordinal);

        return (
            sign + figure[..cut],
            string.Concat(figure[cut..], NoBreakSpace.ToString(), CurrencyOf(currency)));
    }

    /// <summary>
    /// <c>€</c> for the euro, the ISO code for every other currency. Decided by
    /// the booking's currency alone and never by the culture: the symbol a
    /// machine's culture would pick is a lie about a booking in another
    /// currency, and the code is unambiguous where a symbol such as <c>$</c> is
    /// not.
    /// </summary>
    /// <param name="currency">The ISO 4217 code the amount is in.</param>
    private static string CurrencyOf(string currency)
        => string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase) ? "€" : currency;

    /// <summary>
    /// Formats a booking date, without its time. The export carries a time on
    /// some rows and not on others, so a column showing one row's time next to
    /// another row's midnight would be noise rather than information.
    /// </summary>
    /// <param name="bookedOn">The date the booking was posted.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string Date(DateTime bookedOn, IFormatProvider? formatProvider = null)
    {
        return DateOnly.FromDateTime(bookedOn)
            .ToString("d", formatProvider ?? CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Writes how many bookings there are, grouped so a figure in the thousands
    /// stays readable at a glance.
    /// </summary>
    /// <param name="count">The number of bookings.</param>
    /// <param name="formatProvider">The culture to format in.</param>
    public static string Count(int count, IFormatProvider? formatProvider = null)
    {
        return count.ToString("N0", formatProvider ?? CultureInfo.CurrentCulture);
    }
}
