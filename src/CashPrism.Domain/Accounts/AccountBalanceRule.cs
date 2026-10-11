using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;

namespace CashPrism.Domain.Accounts;

/// <summary>
/// How the balance of an account follows from an export: the
/// <c>Kontostand</c> of the account's topmost row that is not dated after the
/// day the export was taken.
/// </summary>
/// <remarks>
/// <para>
/// <c>Kontostand</c> is not a running balance, so no sum over the rows gives the
/// balance. What it does carry, on the row the export lists first for an
/// account, is the balance Finanzguru shows for that account — except where
/// rows dated after the export day are listed above it, which already carry
/// their own. Skipping those matched the app for every account of one measured
/// export; the measurement is in <c>docs/finanzguru-export.md</c>, as a
/// snapshot rather than a specification.
/// </para>
/// <para>
/// Two rows on the same day are told apart by the file alone, which is why the
/// rule runs on the rows in the order the export lists them.
/// </para>
/// </remarks>
public static class AccountBalanceRule
{
    /// <summary>
    /// The balance of every account <paramref name="bookingsInFileOrder"/>
    /// carries a row for, in the order each account first occurs.
    /// </summary>
    /// <param name="run">
    /// The run that read the export. Its export date is the cut-off; where the
    /// sheet name carried none, the day of the import stands in — an export
    /// cannot have been taken after it was imported.
    /// </param>
    /// <param name="bookingsInFileOrder">The export's rows, topmost first.</param>
    public static IReadOnlyList<AccountBalance> For(ImportRun run, IEnumerable<Booking> bookingsInFileOrder)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(bookingsInFileOrder);

        var cutOff = run.ExportedOn ?? DateOnly.FromDateTime(run.ImportedAt.UtcDateTime);

        return [.. bookingsInFileOrder
            .GroupBy(booking => booking.AccountReference, StringComparer.Ordinal)
            .Select(rows => For(run.Id, rows.ToList(), cutOff))];
    }

    private static AccountBalance For(Guid runId, List<Booking> rows, DateOnly cutOff)
    {
        // An account whose rows all lie after the cut-off still has a balance:
        // its topmost row is the best the export offers.
        var balanceRow = rows.FirstOrDefault(row => DateOnly.FromDateTime(row.BookedOn) <= cutOff) ?? rows[0];

        // The newest booking names the account. MaxBy keeps the first of equals,
        // so a tie goes to the row listed higher.
        var newest = rows.MaxBy(row => row.BookedOn)!;

        return new AccountBalance(
            runId,
            balanceRow.AccountReference,
            newest.AccountName,
            balanceRow.ReportedBalanceInCents,
            balanceRow.Currency,
            newest.BookedOn);
    }
}
