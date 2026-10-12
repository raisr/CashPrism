namespace CashPrism.Domain.Accounts;

/// <summary>
/// The account groups in their order, each with its accounts in their order,
/// and the rules for changing them.
/// </summary>
/// <remarks>
/// <para>
/// An assignment can point at an account the latest export does not carry:
/// it is kept, so the account returns to its place once an export brings it
/// back, but it is not shown. <see cref="Place"/> says which accounts are
/// shown, and every rule here counts only those — a group whose accounts are
/// all hidden is empty, and moving an account skips the hidden ones beside it.
/// Until <see cref="Place"/> is called, every assigned account counts as shown.
/// </para>
/// <para>
/// There is always at least one group, so a new account always has a place.
/// </para>
/// </remarks>
public sealed class AccountGrouping
{
    private readonly List<Entry> groups;
    private HashSet<string> shown;

    /// <summary>
    /// Creates the grouping from what was stored.
    /// </summary>
    /// <param name="groups">The groups, in any order; their positions decide it.</param>
    /// <param name="assignments">The assignments, in any order; their positions decide it.</param>
    /// <exception cref="ArgumentException">
    /// There is no group, or an assignment points at a group that is not there.
    /// </exception>
    public AccountGrouping(IEnumerable<AccountGroup> groups, IEnumerable<AccountAssignment> assignments)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(assignments);

        this.groups = [.. groups.OrderBy(group => group.Position).Select(group => new Entry(group.Id, group.Name))];

        if (this.groups.Count == 0)
        {
            throw new ArgumentException("A grouping needs at least one group.", nameof(groups));
        }

        foreach (var assignment in assignments.OrderBy(assignment => assignment.Position))
        {
            var group = Find(assignment.GroupId)
                ?? throw new ArgumentException(
                    $"An assignment points at group {assignment.GroupId}, which is not there.",
                    nameof(assignments));

            group.Accounts.Add(assignment.AccountReference);
        }

        shown = [.. this.groups.SelectMany(group => group.Accounts)];
    }

    /// <summary>The groups, the topmost first, each carrying its position.</summary>
    public IReadOnlyList<AccountGroup> Groups
        => [.. groups.Select((group, position) => new AccountGroup(group.Id, group.Name, position))];

    /// <summary>
    /// Every assignment, hidden accounts included, each carrying its position
    /// inside its group — what is stored.
    /// </summary>
    public IReadOnlyList<AccountAssignment> Assignments
        => [.. groups.SelectMany(group => group.Accounts.Select(
            (account, position) => new AccountAssignment(account, group.Id, position)))];

    /// <summary>
    /// The grouping a fresh database starts with: one group.
    /// </summary>
    /// <param name="id">Identifies the group.</param>
    /// <param name="name">What the group is called.</param>
    public static AccountGrouping Start(Guid id, string name)
        => new([new AccountGroup(id, name, 0)], []);

    /// <summary>
    /// The shown accounts of a group, in their order.
    /// </summary>
    /// <param name="groupId">The group.</param>
    /// <exception cref="ArgumentException">No group has that identity.</exception>
    public IReadOnlyList<string> AccountsIn(Guid groupId)
    {
        var group = Find(groupId)
            ?? throw new ArgumentException($"There is no group {groupId}.", nameof(groupId));

        return [.. group.Accounts.Where(shown.Contains)];
    }

    /// <summary>
    /// Says which accounts are shown, and gives every one of them that is in no
    /// group yet a place: at the end of the topmost group, in the order given.
    /// </summary>
    /// <param name="accountReferences">The accounts the latest export carries.</param>
    public void Place(IEnumerable<string> accountReferences)
    {
        ArgumentNullException.ThrowIfNull(accountReferences);

        var accounts = accountReferences.Distinct(StringComparer.Ordinal).ToList();
        var assigned = groups.SelectMany(group => group.Accounts).ToHashSet(StringComparer.Ordinal);

        groups[0].Accounts.AddRange(accounts.Where(account => !assigned.Contains(account)));
        shown = [.. accounts];
    }

    /// <summary>
    /// Adds a group at the bottom.
    /// </summary>
    /// <param name="id">Identifies the new group.</param>
    /// <param name="name">What it is called; surrounding white space is dropped.</param>
    /// <exception cref="ArgumentException">A group with that identity already exists.</exception>
    public AccountGroupingOutcome Add(Guid id, string name)
    {
        if (Find(id) is not null)
        {
            throw new ArgumentException($"There already is a group {id}.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountGroupingOutcome.NameMissing;
        }

        groups.Add(new Entry(id, name.Trim()));

        return AccountGroupingOutcome.Done;
    }

    /// <summary>
    /// Gives a group another name.
    /// </summary>
    /// <param name="id">The group.</param>
    /// <param name="name">The new name; surrounding white space is dropped.</param>
    public AccountGroupingOutcome Rename(Guid id, string name)
    {
        if (Find(id) is not { } group)
        {
            return AccountGroupingOutcome.GroupNotFound;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return AccountGroupingOutcome.NameMissing;
        }

        group.Name = name.Trim();

        return AccountGroupingOutcome.Done;
    }

    /// <summary>Moves a group one place up; the topmost stays where it is.</summary>
    /// <param name="id">The group.</param>
    public AccountGroupingOutcome MoveUp(Guid id) => MoveGroup(id, -1);

    /// <summary>Moves a group one place down; the bottom one stays where it is.</summary>
    /// <param name="id">The group.</param>
    public AccountGroupingOutcome MoveDown(Guid id) => MoveGroup(id, +1);

    /// <summary>
    /// Deletes a group — only while it shows no account and is not the only
    /// one. The assignments of hidden accounts go with it, so such an account
    /// returns to the topmost group, like a new one.
    /// </summary>
    /// <param name="id">The group.</param>
    public AccountGroupingOutcome Delete(Guid id)
    {
        if (Find(id) is not { } group)
        {
            return AccountGroupingOutcome.GroupNotFound;
        }

        if (group.Accounts.Any(shown.Contains))
        {
            return AccountGroupingOutcome.GroupNotEmpty;
        }

        if (groups.Count == 1)
        {
            return AccountGroupingOutcome.LastGroup;
        }

        groups.Remove(group);

        return AccountGroupingOutcome.Done;
    }

    /// <summary>
    /// Puts an account into a group, at its end. An account already in that
    /// group keeps its place.
    /// </summary>
    /// <param name="accountReference">The account; it has to be shown.</param>
    /// <param name="groupId">The group.</param>
    public AccountGroupingOutcome Assign(string accountReference, Guid groupId)
    {
        if (Find(groupId) is not { } target)
        {
            return AccountGroupingOutcome.GroupNotFound;
        }

        if (GroupOf(accountReference) is not { } current)
        {
            return AccountGroupingOutcome.AccountNotFound;
        }

        if (current != target)
        {
            current.Accounts.Remove(accountReference);
            target.Accounts.Add(accountReference);
        }

        return AccountGroupingOutcome.Done;
    }

    /// <summary>Moves an account one place up inside its group, past the hidden ones.</summary>
    /// <param name="accountReference">The account; it has to be shown.</param>
    public AccountGroupingOutcome MoveAccountUp(string accountReference) => MoveAccount(accountReference, -1);

    /// <summary>Moves an account one place down inside its group, past the hidden ones.</summary>
    /// <param name="accountReference">The account; it has to be shown.</param>
    public AccountGroupingOutcome MoveAccountDown(string accountReference) => MoveAccount(accountReference, +1);

    private AccountGroupingOutcome MoveGroup(Guid id, int step)
    {
        if (Find(id) is not { } group)
        {
            return AccountGroupingOutcome.GroupNotFound;
        }

        var from = groups.IndexOf(group);
        var to = from + step;

        if (to >= 0 && to < groups.Count)
        {
            (groups[from], groups[to]) = (groups[to], groups[from]);
        }

        return AccountGroupingOutcome.Done;
    }

    private AccountGroupingOutcome MoveAccount(string accountReference, int step)
    {
        if (GroupOf(accountReference) is not { } group)
        {
            return AccountGroupingOutcome.AccountNotFound;
        }

        var accounts = group.Accounts;
        var from = accounts.IndexOf(accountReference);

        // The neighbour is the next shown account: swapping with a hidden one
        // would change nothing the person can see.
        for (var to = from + step; to >= 0 && to < accounts.Count; to += step)
        {
            if (shown.Contains(accounts[to]))
            {
                (accounts[from], accounts[to]) = (accounts[to], accounts[from]);

                break;
            }
        }

        return AccountGroupingOutcome.Done;
    }

    private Entry? Find(Guid id) => groups.Find(group => group.Id == id);

    private Entry? GroupOf(string accountReference)
        => shown.Contains(accountReference)
            ? groups.Find(group => group.Accounts.Contains(accountReference, StringComparer.Ordinal))
            : null;

    private sealed class Entry(Guid id, string name)
    {
        public Guid Id { get; } = id;

        public string Name { get; set; } = name;

        public List<string> Accounts { get; } = [];
    }
}
