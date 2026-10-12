namespace CashPrism.Domain.Accounts;

/// <summary>
/// A group of accounts the person sorted together — own, shared, someone
/// else's. Nothing in an export says which group an account is in, so the
/// groups are CashPrism's own state, not a projection.
/// </summary>
public sealed class AccountGroup
{
    /// <summary>
    /// Creates a group.
    /// </summary>
    /// <param name="id">Identifies the group.</param>
    /// <param name="name">What the person called it.</param>
    /// <param name="position">Where it stands among the groups, the topmost at zero.</param>
    /// <exception cref="ArgumentException">A required value is missing.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is negative.</exception>
    public AccountGroup(Guid id, string name, int position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        if (id == Guid.Empty)
        {
            throw new ArgumentException("A group needs an identity.", nameof(id));
        }

        Id = id;
        Name = name;
        Position = position;
    }

    /// <summary>Identifies the group.</summary>
    public Guid Id { get; }

    /// <summary>What the person called it.</summary>
    public string Name { get; }

    /// <summary>Where it stands among the groups, the topmost at zero.</summary>
    public int Position { get; }
}
