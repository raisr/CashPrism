using CashPrism.Application.Access;
using CashPrism.Domain.Access;

namespace CashPrism.Application.Tests.Unit.Access;

/// <summary>Keeps the credential in a field.</summary>
internal sealed class FakeCredentialStore : ICredentialStore
{
    public Credential? Credential { get; private set; }

    public static FakeCredentialStore Holding(string passwordHash)
    {
        var store = new FakeCredentialStore();
        store.Credential = new Credential(passwordHash, DateTimeOffset.UnixEpoch);

        return store;
    }

    public Task<Credential?> GetAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Credential);

    public Task AddAsync(Credential credential, CancellationToken cancellationToken = default)
    {
        Credential = credential;

        return Task.CompletedTask;
    }
}
