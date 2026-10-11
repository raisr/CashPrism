using CashPrism.Application.Bookings;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// The functions a query may call that SQLite does not have, mapped by name
/// onto the ones <see cref="SearchFunctionsInterceptor"/> gives every
/// connection. They exist only to be translated: called in .NET, they throw.
/// </summary>
/// <remarks>
/// A function rather than a folded copy of each searched column: the rule in
/// <see cref="SearchText"/> may change once another source arrives, and a
/// function applies the new rule to every stored booking at once.
/// </remarks>
public static class SearchFunctions
{
    /// <summary>The name <see cref="Fold"/> is known by in SQL.</summary>
    public const string FoldName = "cp_fold";

    /// <summary>
    /// <see cref="SearchText.Fold"/>, run by SQLite on a stored column.
    /// </summary>
    /// <param name="text">The column.</param>
    /// <exception cref="NotSupportedException">Always, when called outside a query.</exception>
    public static string Fold(string text)
        => throw new NotSupportedException($"{nameof(Fold)} runs in the database only, inside a query.");
}
