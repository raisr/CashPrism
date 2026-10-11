using CashPrism.Web.Accounts;

namespace CashPrism.Web.Tests.Unit.Accounts;

public sealed class IbanMaskTests
{
    public sealed class Mask
    {
        [Fact]
        public void Shows_The_Last_Four_Characters_Of_An_Iban()
            => Assert.Equal("•••• 2051", IbanMask.Mask("DE02120300000000202051"));

        [Fact]
        public void Ignores_The_Spaces_An_Iban_Is_Written_With()
            => Assert.Equal("•••• 2051", IbanMask.Mask("DE02 1203 0000 0000 2020 51"));

        [Fact]
        public void Shows_A_Short_Handle_As_It_Stands()
            => Assert.Equal("Paynet", IbanMask.Mask("Paynet"));

        [Fact]
        public void Cuts_A_Long_Identifier_Like_An_Iban()
            => Assert.Equal("•••• 994d", IbanMask.Mask("c01b02f6-e023-a71a-60ea-c59d788e994d"));
    }
}
