using CashPrism.Web.Access;

namespace CashPrism.Web.Tests.Unit.Access;

public sealed class ReturnUrlTests
{
    public sealed class Resolve
    {
        [Theory]
        [InlineData("/")]
        [InlineData("/bookings")]
        [InlineData("/bookings?page=2")]
        public void Keeps_A_Path_Within_The_Application(string returnUrl)
        {
            Assert.Equal(returnUrl, ReturnUrl.Resolve(returnUrl));
        }

        [Theory]
        [InlineData("https://example.org/")]
        [InlineData("//example.org")]
        [InlineData("/\\example.org")]
        [InlineData("bookings")]
        public void Replaces_An_Address_Elsewhere_With_The_Start_Page(string returnUrl)
        {
            Assert.Equal(ReturnUrl.Fallback, ReturnUrl.Resolve(returnUrl));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Replaces_Nothing_With_The_Start_Page(string? returnUrl)
        {
            Assert.Equal(ReturnUrl.Fallback, ReturnUrl.Resolve(returnUrl));
        }
    }
}
