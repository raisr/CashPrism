using CashPrism.Application.Time;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Access;

/// <summary>
/// Sets the first password, for whoever holds the <see cref="SetupCode"/> of the
/// current start.
/// </summary>
public sealed class PasswordSetup
{
    private readonly ICredentialStore store;
    private readonly IPasswordHasher hasher;
    private readonly SetupCode setupCode;
    private readonly IClock clock;

    /// <summary>
    /// Creates the use case.
    /// </summary>
    /// <param name="store">Where the credential is kept.</param>
    /// <param name="hasher">Hashes the password before it is kept.</param>
    /// <param name="setupCode">The code this start printed.</param>
    /// <param name="clock">Says when the password was set.</param>
    public PasswordSetup(ICredentialStore store, IPasswordHasher hasher, SetupCode setupCode, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(hasher);
        ArgumentNullException.ThrowIfNull(setupCode);
        ArgumentNullException.ThrowIfNull(clock);

        this.store = store;
        this.hasher = hasher;
        this.setupCode = setupCode;
        this.clock = clock;
    }

    /// <summary>Whether no password is set yet, so CashPrism waits for one.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<bool> IsPendingAsync(CancellationToken cancellationToken = default)
    {
        return await store.GetAsync(cancellationToken) is null;
    }

    /// <summary>
    /// Sets <paramref name="password"/> as the password, if <paramref name="code"/>
    /// is this start's setup code and no password is set yet.
    /// </summary>
    /// <param name="code">The setup code as typed, or <c>null</c> when none was.</param>
    /// <param name="password">The password as typed.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    public async Task<PasswordSetupOutcome> SetAsync(
        string? code,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(password);

        // Checked first: once a password is set the code is worth nothing, and
        // whoever is guessing it learns nothing about whether a guess was right.
        if (!await IsPendingAsync(cancellationToken))
        {
            return PasswordSetupOutcome.AlreadySet;
        }

        if (!setupCode.Matches(code))
        {
            return PasswordSetupOutcome.WrongCode;
        }

        if (!PasswordRule.IsAcceptable(password))
        {
            return PasswordSetupOutcome.TooShort;
        }

        await store.AddAsync(new Credential(hasher.Hash(password), clock.UtcNow), cancellationToken);

        return PasswordSetupOutcome.Done;
    }
}
