using System.Globalization;

namespace CashPrism.Application.Bookings;

/// <summary>
/// What a person typed into the search field of the booking list, read into
/// the words to look for and the bounds on the amount. Every word and every
/// bound must hold for a booking to be found.
/// </summary>
/// <remarks>
/// Two searches are the same when their text is: the words and bounds follow
/// from it, and the text is what the URL carries.
/// </remarks>
public sealed record BookingSearch
{
    private static readonly (string Symbol, AmountComparison Comparison)[] Operators =
    [
        // The two-character ones first, or ">=" would read as ">" and "=200".
        (">=", AmountComparison.GreaterThanOrEqual),
        ("<=", AmountComparison.LessThanOrEqual),
        (">", AmountComparison.GreaterThan),
        ("<", AmountComparison.LessThan),
        ("=", AmountComparison.Equal),
    ];

    private static readonly string[] Currencies = ["EUR", "€"];

    private BookingSearch(string text, IReadOnlyList<string> words, IReadOnlyList<AmountBound> amountBounds)
    {
        Text = text;
        Words = words;
        AmountBounds = amountBounds;
    }

    /// <summary>The search as it was typed, without surrounding spaces.</summary>
    public string Text { get; }

    /// <summary>
    /// The words to look for, as typed. Each must occur in the counterparty,
    /// the payment reference or the counterparty's IBAN, as part of a word.
    /// </summary>
    public IReadOnlyList<string> Words { get; }

    /// <summary>The bounds on the amount, all of which must hold.</summary>
    public IReadOnlyList<AmountBound> AmountBounds { get; }

    /// <summary>
    /// Reads a search, or <see langword="null"/> when there is nothing to
    /// search for. Never fails: a word that looks like an amount but is not
    /// one — <c>&gt;abc</c> — is searched for as a word.
    /// </summary>
    /// <param name="text">What was typed into the search field.</param>
    public static BookingSearch? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var words = new List<string>();
        var bounds = new List<AmountBound>();
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        for (var index = 0; index < tokens.Length; index++)
        {
            if (Bound(tokens[index]) is not { } bound)
            {
                words.Add(tokens[index]);
                continue;
            }

            bounds.Add(bound);

            // "> 200 €" with the currency on its own belongs to the bound
            // before it; searched for as a word, it would find nothing.
            if (index + 1 < tokens.Length && IsCurrency(tokens[index + 1]))
            {
                index++;
            }
        }

        return new BookingSearch(text.Trim(), words, bounds);
    }

    /// <inheritdoc />
    public bool Equals(BookingSearch? other) => other is not null && Text == other.Text;

    /// <inheritdoc />
    public override int GetHashCode() => Text.GetHashCode(StringComparison.Ordinal);

    private static AmountBound? Bound(string token)
    {
        foreach (var (symbol, comparison) in Operators)
        {
            if (token.StartsWith(symbol, StringComparison.Ordinal))
            {
                return Cents(token[symbol.Length..]) is { } cents ? new AmountBound(comparison, cents) : null;
            }
        }

        return null;
    }

    /// <summary>
    /// A number with a comma or a point before at most two decimals, a
    /// currency behind it allowed, in whole cents. Anything else — a
    /// thousands separator, a sign, a third decimal — is no amount.
    /// </summary>
    private static long? Cents(string number)
    {
        foreach (var currency in Currencies)
        {
            if (number.EndsWith(currency, StringComparison.OrdinalIgnoreCase))
            {
                number = number[..^currency.Length];
                break;
            }
        }

        var parts = number.Replace(',', '.').Split('.');

        if (parts.Length > 2
            || parts[0].Length == 0
            || !parts[0].All(char.IsAsciiDigit)
            || (parts.Length == 2 && (parts[1].Length is < 1 or > 2 || !parts[1].All(char.IsAsciiDigit))))
        {
            return null;
        }

        return decimal.TryParse(string.Join('.', parts), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var euros)
            && euros <= long.MaxValue / 100
                ? (long)(euros * 100)
                : null;
    }

    private static bool IsCurrency(string token)
        => Currencies.Any(currency => string.Equals(token, currency, StringComparison.OrdinalIgnoreCase));
}
