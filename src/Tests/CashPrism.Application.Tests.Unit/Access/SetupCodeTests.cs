using CashPrism.Application.Access;

namespace CashPrism.Application.Tests.Unit.Access;

public sealed class SetupCodeTests
{
    private const string ACode = "K7QFM2XP9HTR";

    public sealed class Constructor
    {
        [Theory]
        [InlineData("K7QFM2XP9HT")]
        [InlineData("K7QFM2XP9HTRS")]
        public void Rejects_A_Code_Of_The_Wrong_Length(string value)
        {
            Assert.Throws<ArgumentException>(() => new SetupCode(value));
        }

        [Fact]
        public void Rejects_A_Character_Outside_The_Alphabet()
        {
            Assert.Throws<ArgumentException>(() => new SetupCode("K7QFM2XP9HT0"));
        }
    }

    public sealed class Display
    {
        [Fact]
        public void Groups_The_Code_In_Fours()
        {
            Assert.Equal("K7QF-M2XP-9HTR", new SetupCode(ACode).Display);
        }
    }

    public sealed class Matches
    {
        [Fact]
        public void Accepts_The_Code_As_Displayed()
        {
            Assert.True(new SetupCode(ACode).Matches("K7QF-M2XP-9HTR"));
        }

        [Fact]
        public void Accepts_The_Code_In_Lower_Case_With_Spaces()
        {
            Assert.True(new SetupCode(ACode).Matches("k7qf m2xp 9htr"));
        }

        [Fact]
        public void Refuses_Another_Code()
        {
            Assert.False(new SetupCode(ACode).Matches("K7QF-M2XP-9HTS"));
        }

        [Fact]
        public void Refuses_Nothing()
        {
            Assert.False(new SetupCode(ACode).Matches(null));
        }
    }
}
