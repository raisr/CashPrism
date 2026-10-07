using CashPrism.Domain.Access;

namespace CashPrism.Domain.Tests.Unit.Access;

public sealed class CredentialTests
{
    public sealed class Constructor
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Rejects_A_Missing_Hash(string passwordHash)
        {
            Assert.Throws<ArgumentException>(() => new Credential(passwordHash, DateTimeOffset.UnixEpoch));
        }

        [Fact]
        public void Keeps_The_Hash()
        {
            var credential = new Credential("a-hash", DateTimeOffset.UnixEpoch);

            Assert.Equal("a-hash", credential.PasswordHash);
        }
    }
}
