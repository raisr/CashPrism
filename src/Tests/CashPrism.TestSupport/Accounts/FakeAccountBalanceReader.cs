using CashPrism.Application.Accounts;
using CashPrism.Domain.Accounts;

namespace CashPrism.TestSupport.Accounts;

/// <summary>
/// Stands in for the stored balances: it answers with the ones it was given.
/// Which run is the latest is the real reader's question, not this one's.
/// </summary>
public sealed class FakeAccountBalanceReader : IAccountBalanceReader
{
    /// <summary>A run id every balance shares unless the test names one.</summary>
    public static readonly Guid ARunId = Guid.Parse("5b0d7c3e-2f41-4a6b-9c8d-7e6f5a4b3c2d");

    /// <summary>The balances the fake answers with.</summary>
    public List<AccountBalance> Balances { get; } = [];

    /// <summary>Adds a balance with plain values for everything a test does not name.</summary>
    public AccountBalance Add(
        string accountName,
        long balanceInCents,
        string accountReference = "DE02120300000000202051",
        DateTime? latestBookedOn = null)
    {
        var balance = new AccountBalance(
            ARunId,
            accountReference,
            accountName,
            balanceInCents,
            "EUR",
            latestBookedOn ?? new DateTime(2026, 10, 1));

        Balances.Add(balance);

        return balance;
    }

    public Task<IReadOnlyList<AccountBalance>> ReadLatestAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AccountBalance>>(Balances);
}
