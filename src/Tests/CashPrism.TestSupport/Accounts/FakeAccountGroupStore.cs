using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;

namespace CashPrism.TestSupport.Accounts;

/// <summary>
/// Stands in for the stored account groups: it keeps what was saved as the
/// groups and assignments a database would hold, and hands out a fresh
/// grouping built from them on every load, as a database would.
/// </summary>
public sealed class FakeAccountGroupStore : IAccountGroupStore
{
    private List<AccountGroup> groups = [];
    private List<AccountAssignment> assignments = [];

    /// <summary>How often a grouping was saved.</summary>
    public int Saves { get; private set; }

    /// <summary>The stored groups, the topmost first.</summary>
    public IReadOnlyList<AccountGroup> Groups => groups;

    /// <summary>The stored assignments.</summary>
    public IReadOnlyList<AccountAssignment> Assignments => assignments;

    /// <summary>Stores a group at the bottom, as if a person had added it before.</summary>
    /// <param name="name">What it is called.</param>
    /// <param name="accountReferences">The accounts in it, in their order.</param>
    /// <returns>The identity of the group.</returns>
    public Guid AddGroup(string name, params string[] accountReferences)
    {
        var id = Guid.NewGuid();
        groups.Add(new AccountGroup(id, name, groups.Count));
        assignments.AddRange(accountReferences.Select((reference, position) => new AccountAssignment(reference, id, position)));

        return id;
    }

    /// <summary>Forgets every group, as deleting them with the data does.</summary>
    public void Clear()
    {
        groups = [];
        assignments = [];
    }

    public Task<AccountGrouping?> LoadAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(groups.Count == 0 ? null : new AccountGrouping(groups, assignments));

    public Task SaveAsync(AccountGrouping grouping, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grouping);

        groups = [.. grouping.Groups];
        assignments = [.. grouping.Assignments];
        Saves++;

        return Task.CompletedTask;
    }
}
