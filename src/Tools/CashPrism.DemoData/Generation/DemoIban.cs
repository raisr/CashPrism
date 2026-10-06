using System.Globalization;
using System.Numerics;
using System.Text;

namespace CashPrism.DemoData.Generation;

/// <summary>
/// Fictional German account and creditor identifiers that nonetheless carry a
/// correct ISO 7064 check number, so they look like what a bank transmits.
/// </summary>
public static class DemoIban
{
    /// <summary>
    /// The bank code every demo IBAN uses. Made up; the check number is right,
    /// the account behind it does not exist.
    /// </summary>
    public const string BankCode = "99950000";

    /// <summary>The demo IBAN of the account holder named <paramref name="holder"/>.</summary>
    public static string For(string holder) => German(BankCode + DemoIds.Digits(holder + "|account", 10));

    /// <summary>
    /// A German IBAN for <paramref name="basicBankAccountNumber"/>, the 18 digits
    /// of bank code and account number.
    /// </summary>
    public static string German(string basicBankAccountNumber)
        => $"DE{CheckNumber(basicBankAccountNumber):00}{basicBankAccountNumber}";

    /// <summary>
    /// A German SEPA creditor identifier for <paramref name="nationalId"/>. The
    /// business code <c>ZZZ</c> is not part of the check number.
    /// </summary>
    public static string CreditorId(string nationalId) => $"DE{CheckNumber(nationalId):00}ZZZ{nationalId}";

    /// <summary>Whether <paramref name="iban"/> carries a correct check number.</summary>
    public static bool IsValid(string iban)
        => iban.Length > 4 && Remainder(iban[4..] + iban[..4]) == 1;

    private static int CheckNumber(string body) => 98 - Remainder(body + "DE00");

    private static int Remainder(string text)
    {
        var digits = new StringBuilder();

        foreach (var character in text)
        {
            digits.Append(char.IsDigit(character)
                ? character.ToString()
                : (character - 'A' + 10).ToString(CultureInfo.InvariantCulture));
        }

        return (int)(BigInteger.Parse(digits.ToString(), CultureInfo.InvariantCulture) % 97);
    }
}
