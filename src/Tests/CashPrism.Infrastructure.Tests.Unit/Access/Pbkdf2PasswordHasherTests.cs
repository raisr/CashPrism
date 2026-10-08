using CashPrism.Infrastructure.Access;

namespace CashPrism.Infrastructure.Tests.Unit.Access;

public sealed class Pbkdf2PasswordHasherTests
{
    private const string ThePassword = "korrekt pferd batterie";

    public sealed class Hash
    {
        [Fact]
        public void Does_Not_Contain_The_Password()
        {
            var hash = new Pbkdf2PasswordHasher().Hash(ThePassword);

            Assert.DoesNotContain(ThePassword, hash, StringComparison.Ordinal);
        }

        [Fact]
        public void Salts_Every_Hash_Differently()
        {
            var hasher = new Pbkdf2PasswordHasher();

            Assert.NotEqual(hasher.Hash(ThePassword), hasher.Hash(ThePassword));
        }

        [Fact]
        public void Names_The_Algorithm_And_The_Iterations()
        {
            var hash = new Pbkdf2PasswordHasher().Hash(ThePassword);

            Assert.StartsWith("pbkdf2-sha512$210000$", hash, StringComparison.Ordinal);
        }
    }

    public sealed class Verify
    {
        [Fact]
        public void Accepts_The_Password_The_Hash_Was_Made_From()
        {
            var hasher = new Pbkdf2PasswordHasher();

            Assert.True(hasher.Verify(ThePassword, hasher.Hash(ThePassword)));
        }

        [Fact]
        public void Rejects_Another_Password()
        {
            var hasher = new Pbkdf2PasswordHasher();

            Assert.False(hasher.Verify("falsches pferd batterie", hasher.Hash(ThePassword)));
        }

        // A hash stored with fewer iterations than this build uses still checks:
        // the count is read from the stored value, not from the build.
        [Fact]
        public void Accepts_A_Hash_Stored_With_Another_Iteration_Count()
        {
            const string StoredWithOneThousandIterations =
                "pbkdf2-sha512$1000$AAECAwQFBgcICQoLDA0ODw==$eI/zsT+dVVEd/gSokWSH8qcpJpNq/BuMFVvnGfbfA88=";

            var accepted = new Pbkdf2PasswordHasher().Verify(ThePassword, StoredWithOneThousandIterations);

            Assert.True(accepted);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not a hash")]
        [InlineData("sha1$1000$AAECAwQFBgcICQoLDA0ODw==$AAAA")]
        [InlineData("pbkdf2-sha512$0$AAECAwQFBgcICQoLDA0ODw==$AAAA")]
        [InlineData("pbkdf2-sha512$1000$not base64$AAAA")]
        public void Rejects_A_Stored_Value_It_Cannot_Read(string stored)
        {
            Assert.False(new Pbkdf2PasswordHasher().Verify(ThePassword, stored));
        }
    }
}
