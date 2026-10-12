namespace CashPrism.Domain.Accounts;

/// <summary>
/// Which group an account is in, and where it stands inside it.
/// </summary>
/// <remarks>
/// Keyed by the account reference rather than by anything an import stored,
/// so an assignment outlives the bookings: delete them, import again, and the
/// account is back where it was.
/// </remarks>
public sealed class AccountAssignment
{
    /// <summary>
    /// Creates an assignment.
    /// </summary>
    /// <param name="accountReference">The account — an IBAN, or the handle a provider account is identified by.</param>
    /// <param name="groupId">The group the account is in.</param>
    /// <param name="position">Where it stands inside the group, the topmost at zero.</param>
    /// <exception cref="ArgumentException">A required value is missing.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is negative.</exception>
    public AccountAssignment(string accountReference, Guid groupId, int position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountReference);
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        if (groupId == Guid.Empty)
        {
            throw new ArgumentException("An assignment needs the group it points at.", nameof(groupId));
        }

        AccountReference = accountReference;
        GroupId = groupId;
        Position = position;
    }

    /// <summary>The account.</summary>
    public string AccountReference { get; }

    /// <summary>The group the account is in.</summary>
    public Guid GroupId { get; }

    /// <summary>Where it stands inside the group, the topmost at zero.</summary>
    public int Position { get; }
}
