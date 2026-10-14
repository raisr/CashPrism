namespace CashPrism.Application.Accounts;

/// <summary>How one account's balance developed.</summary>
/// <param name="AccountReference">The account.</param>
/// <param name="Holds">
/// Whether the history agrees with the export. When it does not, the account
/// has no points, and the page says why instead of drawing a wrong line.
/// </param>
/// <param name="Points">
/// The balance at the end of each month and on the day the data is current to,
/// oldest first. A month before the account's first booking is left out.
/// </param>
public sealed record AccountBalanceHistory(string AccountReference, bool Holds, IReadOnlyList<DatedBalance> Points);
