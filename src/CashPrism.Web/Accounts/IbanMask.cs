namespace CashPrism.Web.Accounts;

/// <summary>
/// An account number as a list shows it: enough to tell two accounts of the
/// same name apart, not enough to copy. <c>DE02 1203 … 2051</c> becomes
/// <c>•••• 2051</c>.
/// </summary>
/// <remarks>
/// A reference is not always an IBAN: a provider account is identified by a
/// handle such as <c>Paynet</c>, or by a UUID. A short handle is shown as it
/// stands — masking it would leave nothing to recognise — and anything long
/// is cut to its last four characters like an IBAN.
/// </remarks>
public static class IbanMask
{
    private const int Shown = 4;

    // Up to this length a reference is a name rather than a number.
    private const int LongestHandle = 12;

    /// <summary>The masked form of <paramref name="accountReference"/>.</summary>
    /// <param name="accountReference">The account's reference as the export carries it.</param>
    public static string Mask(string accountReference)
    {
        ArgumentNullException.ThrowIfNull(accountReference);

        var compact = accountReference.Replace(" ", string.Empty, StringComparison.Ordinal);

        return compact.Length <= LongestHandle
            ? compact
            : "•••• " + compact[^Shown..];
    }
}
