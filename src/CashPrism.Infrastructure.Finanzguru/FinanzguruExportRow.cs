namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// One data row of a FinanzGuru export.
/// </summary>
/// <remarks>
/// <para>
/// Three columns are carried as their own type because handing them on as text
/// would mean making a decision and hiding it: a date would lose the time
/// component some rows carry, and a money cell would leave the reader to guess
/// how the number rounds to a cent. Both decisions belong where the value comes
/// out of the file, so they are taken here — see
/// <see cref="FinanzguruAmount"/>.
/// </para>
/// <para>
/// Everything else stays in <see cref="Values"/>, which is the verbatim record
/// of what the row said and is what a stored raw row is built from. The three
/// typed columns appear there as well, in a lossless invariant form, so the
/// record stays complete.
/// </para>
/// </remarks>
/// <param name="RowNumber">
/// The one-based worksheet row this came from, so a message can point at the
/// file. Data starts at row 2; row 1 is the header.
/// </param>
/// <param name="BookedOn">
/// The date the booking was posted. The time component is kept where the export
/// carries one: 52 of 6,324 measured rows do, and dropping it belongs where
/// bookings are compared rather than here. See <c>docs/finanzguru-export.md</c>.
/// </param>
/// <param name="AmountInCents">The signed booking amount in whole cents.</param>
/// <param name="BalanceInCents">
/// The balance the export reports for the booking, in whole cents. It looks like
/// a running balance and is not one — nothing may be derived from it.
/// </param>
/// <param name="Values">
/// Every column of the row by header name, as text. Empty cells are present with
/// an empty value, so the set of keys is the same on every row.
/// </param>
public sealed record FinanzguruExportRow(
    int RowNumber,
    DateTime BookedOn,
    long AmountInCents,
    long BalanceInCents,
    IReadOnlyDictionary<string, string> Values);
