using CashPrism.Application.Access;

namespace CashPrism.Application.Tests.Unit.Access;

/// <summary>
/// A hasher whose output can be read in an assertion. What a real hash looks
/// like is the business of the infrastructure's own tests.
/// </summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    public static string HashOf(string password) => $"hashed:{password}";

    public string Hash(string password) => HashOf(password);

    public bool Verify(string password, string hash) => hash == HashOf(password);
}
