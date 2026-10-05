using System.Globalization;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// A dictionary of sequential, human-labelled placeholders — <c>Counterparty 001</c>,
/// <c>Reference 0001</c> — assigned to the distinct values of one or more columns.
/// </summary>
/// <remarks>
/// A dictionary can be built from more than one labelled group, e.g. the party
/// name dictionary assigns <c>Name Referenzkonto</c> as <c>Account 01</c> first
/// and only then assigns whatever is left of <c>Beguenstigter/Auftraggeber</c> as
/// <c>Counterparty 001</c> — the same value in both columns still lands on
/// whichever placeholder its first group assigned. Each group gets its own digit
/// width, sized to that group's own count: the few own accounts stay two digits
/// wide even though hundreds of counterparties need three, because the two
/// labels are visually distinct and never need to line up.
/// </remarks>
public sealed class LabelledPlaceholderDictionary
{
    private readonly Dictionary<string, string> _map = new(StringComparer.Ordinal);

    /// <summary>The replacement assigned to every value seen so far.</summary>
    public IReadOnlyDictionary<string, string> Map => _map;

    /// <summary>
    /// Assigns <paramref name="label"/>-prefixed placeholders to every value in
    /// <paramref name="rawValues"/> not already in the dictionary, in ordinal
    /// sorted order.
    /// </summary>
    public void AssignGroup(IEnumerable<string> rawValues, string label)
    {
        ArgumentNullException.ThrowIfNull(rawValues);
        ArgumentNullException.ThrowIfNull(label);

        var newValues = rawValues
            .Where(value => !_map.ContainsKey(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

        if (newValues.Count == 0)
        {
            return;
        }

        var width = Math.Max(2, newValues.Count.ToString(CultureInfo.InvariantCulture).Length);

        for (var i = 0; i < newValues.Count; i++)
        {
            var number = (i + 1).ToString("D" + width.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

            _map[newValues[i]] = $"{label} {number}";
        }
    }
}
