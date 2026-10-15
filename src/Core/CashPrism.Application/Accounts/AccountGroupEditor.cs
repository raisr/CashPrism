using CashPrism.Domain.Accounts;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Reads the account groups with the accounts of the latest export in them,
/// and changes them one step at a time.
/// </summary>
/// <remarks>
/// An account no group holds yet is placed when the groups are read — at the
/// end of the topmost group, the highest balance first — but that place is
/// only stored with the next change. Until then it follows the topmost group,
/// which is where a new account belongs.
/// </remarks>
public sealed class AccountGroupEditor
{
    private readonly IAccountBalanceReader balances;
    private readonly IAccountGroupStore store;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="balances">Reads the accounts of the latest export.</param>
    /// <param name="store">Keeps the groups.</param>
    public AccountGroupEditor(IAccountBalanceReader balances, IAccountGroupStore store)
    {
        ArgumentNullException.ThrowIfNull(balances);
        ArgumentNullException.ThrowIfNull(store);

        this.balances = balances;
        this.store = store;
    }

    /// <summary>
    /// The groups in their order, each with its accounts and their sum. Where
    /// no group is stored — a fresh database, or one whose groups were deleted
    /// — a single group is started and stored first.
    /// </summary>
    /// <param name="startName">
    /// What that single group is called. The caller names it, because it is
    /// text a person reads in their own language.
    /// </param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<IReadOnlyList<AccountGroupSummary>> ReadAsync(
        string startName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startName);

        var accounts = AccountOrder.ByBalance(await balances.ReadLatestAsync(cancellationToken));
        var grouping = await store.LoadAsync(cancellationToken);

        if (grouping is null)
        {
            grouping = AccountGrouping.Start(Guid.NewGuid(), startName);
            await store.SaveAsync(grouping, cancellationToken);
        }

        grouping.Place(accounts.Select(account => account.AccountReference));

        var byReference = accounts.ToDictionary(account => account.AccountReference, StringComparer.Ordinal);

        return [.. grouping.Groups.Select(group =>
        {
            var shown = grouping.AccountsIn(group.Id).Select(reference => byReference[reference]).ToList();

            return new AccountGroupSummary(group.Id, group.Name, shown, shown.Sum(account => account.BalanceInCents));
        })];
    }

    /// <summary>
    /// Applies one change to the groups and stores the result — only when the
    /// change was made.
    /// </summary>
    /// <param name="change">
    /// The change, such as <c>grouping => grouping.Rename(id, name)</c>. It
    /// sees the accounts of the latest export already placed.
    /// </param>
    /// <param name="cancellationToken">Cancels the change before it is stored.</param>
    /// <returns>
    /// How the change ended; <see cref="AccountGroupingOutcome.GroupNotFound"/>
    /// when no group is stored any more, since the groups were deleted while
    /// the page still showed them.
    /// </returns>
    public async Task<AccountGroupingOutcome> ChangeAsync(
        Func<AccountGrouping, AccountGroupingOutcome> change,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);

        var accounts = AccountOrder.ByBalance(await balances.ReadLatestAsync(cancellationToken));
        var grouping = await store.LoadAsync(cancellationToken);

        if (grouping is null)
        {
            return AccountGroupingOutcome.GroupNotFound;
        }

        grouping.Place(accounts.Select(account => account.AccountReference));

        var outcome = change(grouping);

        if (outcome == AccountGroupingOutcome.Done)
        {
            await store.SaveAsync(grouping, cancellationToken);
        }

        return outcome;
    }
}
