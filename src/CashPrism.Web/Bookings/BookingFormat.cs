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
    /// Always a sign, always two decimals. The sign is what carries the meaning
    /// of a credit or a debit — <c>docs/ui.md</c> keeps the colour a
    /// reinforcement, so the text has to say it on its own.
    /// </summary>
    /// <remarks>
    /// The third section is what makes a zero read as <c>0,00</c> instead of
    /// taking the positive section and claiming to be a credit of nothing.
    /// </remarks>
    private const string SignedWithTwoDecimals = "+#,##0.00;-#,##0.00;0.00";

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
    /// The amount, the currency code behind it after a non-breaking space. The
    /// code rather than a symbol: the symbol of the machine's culture would be a
    /// lie about a booking in another currency.
    /// </returns>
    public static string Amount(long amountInCents, string currency, IFormatProvider? formatProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var amount = (decimal)amountInCents / 100m;

        return string.Concat(
            amount.ToString(SignedWithTwoDecimals, formatProvider ?? CultureInfo.CurrentCulture),
            " ",
            currency);
    }

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
}
