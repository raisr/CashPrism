using CashPrism.Infrastructure.Finanzguru;

namespace CashPrism.Anonymiser.Anonymisation;

/// <summary>
/// Every value dictionary of one anonymiser run, built once from the values
/// read across all input files and then used to rewrite every one of them —
/// see <c>Agents.md</c>'s ticket for why the dictionaries, not the files, are
/// what a run shares.
/// </summary>
public sealed class AnonymisationDictionaries
{
    private readonly ShapePreservingDictionary _accounts;
    private readonly LabelledPlaceholderDictionary _names;
    private readonly LabelledPlaceholderDictionary _references;
    private readonly LabelledPlaceholderDictionary _mandates;
    private readonly LabelledPlaceholderDictionary _creditors;
    private readonly LabelledPlaceholderDictionary _contracts;
    private readonly LabelledPlaceholderDictionary _bookings;
    private readonly LabelledPlaceholderDictionary _tags;
    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _originalValues;

    private AnonymisationDictionaries(
        ShapePreservingDictionary accounts,
        LabelledPlaceholderDictionary names,
        LabelledPlaceholderDictionary references,
        LabelledPlaceholderDictionary mandates,
        LabelledPlaceholderDictionary creditors,
        LabelledPlaceholderDictionary contracts,
        LabelledPlaceholderDictionary bookings,
        LabelledPlaceholderDictionary tags,
        IReadOnlyDictionary<string, IReadOnlySet<string>> originalValues)
    {
        _accounts = accounts;
        _names = names;
        _references = references;
        _mandates = mandates;
        _creditors = creditors;
        _contracts = contracts;
        _bookings = bookings;
        _tags = tags;
        _originalValues = originalValues;
    }

    /// <summary>The column names this run replaces — the other 18 are kept as they are.</summary>
    public static IReadOnlySet<string> ReplacedColumns { get; } = new HashSet<string>(
        [
            FinanzguruColumns.AccountReference,
            FinanzguruColumns.AccountName,
            FinanzguruColumns.Counterparty,
            FinanzguruColumns.CounterpartyIban,
            FinanzguruColumns.PaymentReference,
            FinanzguruColumns.MandateReference,
            FinanzguruColumns.CreditorId,
            FinanzguruColumns.ContractId,
            FinanzguruColumns.BookingId,
            FinanzguruColumns.OriginalReferenceId,
            FinanzguruColumns.Tags,
        ],
        StringComparer.Ordinal);

    /// <summary>
    /// Builds every dictionary from the raw values read from every input file of
    /// the run, one entry per file, each keyed by column name.
    /// </summary>
    public static AnonymisationDictionaries Build(
        IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> valuesByFile)
    {
        ArgumentNullException.ThrowIfNull(valuesByFile);

        var merged = MergeByColumn(valuesByFile);

        var accounts = new ShapePreservingDictionary(DistinctTotal(
            merged[FinanzguruColumns.AccountReference], merged[FinanzguruColumns.CounterpartyIban]));
        accounts.AssignGroup(merged[FinanzguruColumns.AccountReference]);
        accounts.AssignGroup(merged[FinanzguruColumns.CounterpartyIban]);

        var names = new LabelledPlaceholderDictionary();
        names.AssignGroup(merged[FinanzguruColumns.AccountName], "Account");
        names.AssignGroup(merged[FinanzguruColumns.Counterparty], "Counterparty");

        var references = new LabelledPlaceholderDictionary();
        references.AssignGroup(merged[FinanzguruColumns.PaymentReference], "Reference");

        var mandates = new LabelledPlaceholderDictionary();
        mandates.AssignGroup(merged[FinanzguruColumns.MandateReference], "Mandate");

        var creditors = new LabelledPlaceholderDictionary();
        creditors.AssignGroup(merged[FinanzguruColumns.CreditorId], "Creditor");

        var contracts = new LabelledPlaceholderDictionary();
        contracts.AssignGroup(merged[FinanzguruColumns.ContractId], "Contract");

        var bookings = new LabelledPlaceholderDictionary();
        bookings.AssignGroup(
            merged[FinanzguruColumns.BookingId].Concat(merged[FinanzguruColumns.OriginalReferenceId]), "Booking");

        var tags = new LabelledPlaceholderDictionary();
        tags.AssignGroup(merged[FinanzguruColumns.Tags], "Tag");

        var originalValues = merged.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlySet<string>)new HashSet<string>(pair.Value, StringComparer.Ordinal),
            StringComparer.Ordinal);

        return new AnonymisationDictionaries(
            accounts, names, references, mandates, creditors, contracts, bookings, tags, originalValues);
    }

    /// <summary>
    /// The replacement for <paramref name="rawValue"/>, read from
    /// <paramref name="columnName"/>. <paramref name="columnName"/> must be one of
    /// <see cref="ReplacedColumns"/>.
    /// </summary>
    public string Replace(string columnName, string rawValue)
    {
        var map = DictionaryFor(columnName)
            ?? throw new ArgumentOutOfRangeException(nameof(columnName), columnName, "This column is not replaced.");

        if (!map.TryGetValue(rawValue, out var replacement))
        {
            throw new InvalidOperationException(
                $"No replacement was assigned to a value read from column '{columnName}'. " +
                "The pre-pass over the input files did not see this value — this is an internal inconsistency, not a bad input file.");
        }

        return replacement;
    }

    /// <summary>Every distinct raw value <paramref name="columnName"/> carried across the whole run.</summary>
    public IReadOnlySet<string> OriginalValues(string columnName)
        => _originalValues.TryGetValue(columnName, out var values) ? values : new HashSet<string>(StringComparer.Ordinal);

    private IReadOnlyDictionary<string, string>? DictionaryFor(string columnName) => columnName switch
    {
        FinanzguruColumns.AccountReference or FinanzguruColumns.CounterpartyIban => _accounts.Map,
        FinanzguruColumns.AccountName or FinanzguruColumns.Counterparty => _names.Map,
        FinanzguruColumns.PaymentReference => _references.Map,
        FinanzguruColumns.MandateReference => _mandates.Map,
        FinanzguruColumns.CreditorId => _creditors.Map,
        FinanzguruColumns.ContractId => _contracts.Map,
        FinanzguruColumns.BookingId or FinanzguruColumns.OriginalReferenceId => _bookings.Map,
        FinanzguruColumns.Tags => _tags.Map,
        _ => null,
    };

    private static Dictionary<string, List<string>> MergeByColumn(
        IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>> valuesByFile)
    {
        var merged = ReplacedColumns.ToDictionary(column => column, _ => new List<string>(), StringComparer.Ordinal);

        foreach (var file in valuesByFile)
        {
            foreach (var column in ReplacedColumns)
            {
                if (file.TryGetValue(column, out var values))
                {
                    merged[column].AddRange(values);
                }
            }
        }

        return merged;
    }

    private static int DistinctTotal(IEnumerable<string> first, IEnumerable<string> second)
        => first.Concat(second).Distinct(StringComparer.Ordinal).Count();
}
