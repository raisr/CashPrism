using CashPrism.Domain.Access;

namespace CashPrism.Domain.Tests.Unit.Access;

public sealed class PasswordRuleTests
{
    public sealed class IsAcceptable
    {
        [Fact]
        public void Refuses_A_Password_One_Character_Short()
        {
            Assert.False(PasswordRule.IsAcceptable(new string('a', PasswordRule.MinimumLength - 1)));
        }

        [Fact]
        public void Accepts_A_Password_Of_Exactly_The_Minimum_Length()
        {
            Assert.True(PasswordRule.IsAcceptable(new string('a', PasswordRule.MinimumLength)));
        }

        [Fact]
        public void Accepts_A_Password_Without_Digits_Or_Symbols()
        {
            Assert.True(PasswordRule.IsAcceptable("korrekt pferd batterie"));
        }
    }
}
