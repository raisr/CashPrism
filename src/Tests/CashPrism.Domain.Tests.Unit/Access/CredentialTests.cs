using CashPrism.Domain.Access;

namespace CashPrism.Domain.Tests.Unit.Access;

public sealed class CredentialTests
{
    private static readonly DateTimeOffset Later = new(2026, 10, 8, 20, 0, 0, TimeSpan.Zero);

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

        [Fact]
        public void Starts_At_The_First_Generation()
        {
            var credential = new Credential("a-hash", DateTimeOffset.UnixEpoch);

            Assert.Equal(Credential.FirstGeneration, credential.Generation);
        }
    }

    public sealed class SetPassword
    {
        [Fact]
        public void Replaces_The_Hash_And_When_It_Was_Set()
        {
            var credential = new Credential("the-old-hash", DateTimeOffset.UnixEpoch);

            credential.SetPassword("the-new-hash", Later);

            Assert.Equal(("the-new-hash", Later), (credential.PasswordHash, credential.SetAt));
        }

        [Fact]
        public void Raises_The_Generation()
        {
            var credential = new Credential("the-old-hash", DateTimeOffset.UnixEpoch);

            credential.SetPassword("the-new-hash", Later);

            Assert.Equal(Credential.FirstGeneration + 1, credential.Generation);
        }

        [Fact]
        public void Sets_A_Password_Again_After_A_Reset()
        {
            var credential = new Credential("the-old-hash", DateTimeOffset.UnixEpoch);
            credential.Reset();

            credential.SetPassword("the-new-hash", Later);

            Assert.True(credential.IsPasswordSet);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Rejects_A_Missing_Hash(string passwordHash)
        {
            var credential = new Credential("the-old-hash", DateTimeOffset.UnixEpoch);

            Assert.Throws<ArgumentException>(() => credential.SetPassword(passwordHash, Later));
        }
    }

    public sealed class Reset
    {
        [Fact]
        public void Drops_The_Hash()
        {
            var credential = new Credential("a-hash", DateTimeOffset.UnixEpoch);

            credential.Reset();

            Assert.Equal((false, null), (credential.IsPasswordSet, credential.PasswordHash));
        }

        [Fact]
        public void Raises_The_Generation()
        {
            var credential = new Credential("a-hash", DateTimeOffset.UnixEpoch);

            credential.Reset();

            Assert.Equal(Credential.FirstGeneration + 1, credential.Generation);
        }

        [Fact]
        public void Refuses_When_No_Password_Is_Set()
        {
            var credential = new Credential("a-hash", DateTimeOffset.UnixEpoch);
            credential.Reset();

            Assert.Throws<InvalidOperationException>(credential.Reset);
        }
    }
}
