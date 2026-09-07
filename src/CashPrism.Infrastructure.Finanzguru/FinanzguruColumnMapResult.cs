namespace CashPrism.Infrastructure.Finanzguru;

/// <summary>
/// The outcome of resolving a FinanzGuru header row: either the assignment from
/// column name to column index, or the reasons the header row does not fit.
/// "This file does not fit" is an expected outcome of reading a foreign export,
/// so it is reported as a result rather than thrown.
/// </summary>
/// <param name="Columns">
/// The resolved column name to zero-based column index assignment. Empty on
/// failure: a partial map invites reading the wrong column, which is the failure
/// this whole type exists to prevent.
/// </param>
/// <param name="MissingColumns">
/// The known columns the header row does not carry, in export order. Empty on
/// success.
/// </param>
/// <param name="UnknownColumns">
/// The header names that are not known columns, in the order they appear, each
/// listed once. Reported on success as well: what to do with an unexpected
/// column is the caller's decision, not this type's.
/// </param>
/// <param name="DuplicateColumns">
/// The known columns the header row carries more than once, each listed once.
/// Which of the two a reader should take is undecidable, so this fails instead
/// of guessing. Empty on success.
/// </param>
public sealed record FinanzguruColumnMapResult(
    IReadOnlyDictionary<string, int> Columns,
    IReadOnlyList<string> MissingColumns,
    IReadOnlyList<string> UnknownColumns,
    IReadOnlyList<string> DuplicateColumns)
{
    /// <summary>
    /// Whether every known column was found exactly once. Unknown extra columns
    /// do not make a header row unusable and therefore do not affect this.
    /// </summary>
    public bool IsSuccess => MissingColumns.Count == 0 && DuplicateColumns.Count == 0;
}
