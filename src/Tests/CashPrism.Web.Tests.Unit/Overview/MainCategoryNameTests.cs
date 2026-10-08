using CashPrism.Web.Overview;
using CashPrism.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Tests.Unit.Overview;

public sealed class MainCategoryNameTests
{
    public sealed class For
    {
        private static IStringLocalizer<Strings> Text()
            => new ServiceCollection()
                .AddLogging()
                .AddLocalization()
                .BuildServiceProvider()
                .GetRequiredService<IStringLocalizer<Strings>>();

        [Fact]
        public void Writes_The_Umlaut_Finanzguru_Wrote_Out()
            => Assert.Equal("Mobilität", MainCategoryName.For("Mobilitaet", Text()));

        [Fact]
        public void Shows_Any_Other_Name_As_The_Export_Carries_It()
            => Assert.Equal("Essen & Trinken", MainCategoryName.For("Essen & Trinken", Text()));
    }
}
