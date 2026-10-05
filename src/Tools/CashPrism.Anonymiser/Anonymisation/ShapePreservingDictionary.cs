using System.Globalization;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// The one dictionary shared by <c>Referenzkonto</c> and
/// <c>IBAN Beguenstigter/Auftraggeber</c>: an own IBAN that also shows up as a
/// counterparty IBAN gets the same shape-preserving replacement in both
/// columns, because it is the same map either way.
/// </summary>
/// <remarks>
/// Values are assigned in two groups, own accounts before everything else —
/// see <see cref="AnonymisationDictionaries"/> for why — but each group is
/// itself collected in full, deduplicated and sorted ordinally before any
/// index is handed out. That is what makes the assignment independent of the
/// row order the values were read in, including across every input file of one
/// run: this dictionary is deliberately never reused between two separate
/// invocations of the tool.
/// </remarks>
public sealed class ShapePreservingDictionary
{
    private readonly Dictionary<string, string> _map = new(StringComparer.Ordinal);
    private readonly int _emailWidth;
    private int _nextIndex = 1;

    /// <param name="totalDistinctCount">
    /// The number of distinct values this dictionary will end up holding, across
    /// every group it is about to assign. Known upfront so the one shape that
    /// needs a shared digit width — the email fallback — pads consistently
    /// regardless of which group a value happens to be assigned in.
    /// </param>
    public ShapePreservingDictionary(int totalDistinctCount)
    {
        _emailWidth = Math.Max(2, DigitCount(totalDistinctCount));
    }

    /// <summary>The replacement assigned to every value seen so far.</summary>
    public IReadOnlyDictionary<string, string> Map => _map;

    /// <summary>
    /// Assigns a replacement to every value in <paramref name="rawValues"/> not
    /// already in the dictionary, in ordinal sorted order.
    /// </summary>
    public void AssignGroup(IEnumerable<string> rawValues)
    {
        ArgumentNullException.ThrowIfNull(rawValues);

        var newValues = rawValues
            .Where(value => !_map.ContainsKey(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal);

        foreach (var value in newValues)
        {
            _map[value] = ValueShape.BuildReplacement(value, _nextIndex, _emailWidth);
            _nextIndex++;
        }
    }

    private static int DigitCount(int value)
        => value <= 0 ? 1 : value.ToString(CultureInfo.InvariantCulture).Length;
}
