using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.TestSupport.Xlsx;

/// <summary>
/// Builds one data row of a FinanzGuru export, filled in every column a booking
/// is projected from, so a test only states the value it is actually about.
/// </summary>
/// <remarks>
/// The columns left out here are the ones nothing reads yet; an empty cell is
/// what a real export carries in most of them anyway. The ones that must never be
/// empty are the two identities and the three yes/no flags: the identities name
/// the booking and its account, and a flag is translated strictly, so a blank
/// would fail the row rather than default.
/// </remarks>
public static class FinanzguruTestRow
{
    /// <summary>A booking id that is not any other fixture's.</summary>
    public const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";

    /// <summary>A second booking id, for a test that needs two.</summary>
    public const string AnotherFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    /// <summary>
    /// Builds a row. Every parameter has a value a real export could carry, so a
    /// caller overrides only what its assertion is about.
    /// </summary>
    /// <param name="bookingId">The <c>Buchungs-ID</c>, which identifies the booking.</param>
    /// <param name="bookingDate">The <c>Buchungstag</c>, as <c>dd.MM.yyyy</c> or with a time.</param>
    /// <param name="amount">The <c>Betrag</c>, with a decimal point.</param>
    /// <param name="balance">The <c>Kontostand</c>, with a decimal point.</param>
    /// <param name="counterparty">The other party of the booking.</param>
    /// <param name="isInternalTransfer">The <c>Analyse-Umbuchung</c> word: <c>ja</c> or <c>nein</c>.</param>
    /// <param name="splitType">The <c>Split-Typ</c> word, or empty for the ordinary case.</param>
    /// <param name="originalReferenceId">The booking a split part points back to.</param>
    public static Dictionary<string, string> Create(
        string bookingId = AFingerprint,
        string bookingDate = "12.03.2026",
        string amount = "-63.17",
        string balance = "3240.00",
        string counterparty = "Supermarkt",
        string isInternalTransfer = FinanzguruFlag.No,
        string splitType = "",
        string originalReferenceId = "")
        => new(StringComparer.Ordinal)
        {
            [FinanzguruColumns.BookingDate] = bookingDate,
            [FinanzguruColumns.Amount] = amount,
            [FinanzguruColumns.Balance] = balance,
            [FinanzguruColumns.Currency] = "EUR",
            [FinanzguruColumns.AccountReference] = "DE02120300000000202051",
            [FinanzguruColumns.AccountName] = "Girokonto",
            [FinanzguruColumns.Counterparty] = counterparty,
            [FinanzguruColumns.MainCategory] = "Lebensmittel",
            [FinanzguruColumns.SubCategory] = "Supermarkt",
            [FinanzguruColumns.IsInternalTransfer] = isInternalTransfer,
            [FinanzguruColumns.IsContract] = FinanzguruFlag.No,
            [FinanzguruColumns.ExcludedFromDisposableIncome] = FinanzguruFlag.No,
            [FinanzguruColumns.SplitType] = splitType,
            [FinanzguruColumns.OriginalReferenceId] = originalReferenceId,
            [FinanzguruColumns.BookingId] = bookingId,
        };
}
