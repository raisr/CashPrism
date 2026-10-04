using System.Globalization;
using CashPrism.Web.Localisation;
using CashPrism.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Tests.Unit.Localisation;

/// <summary>
/// Every list writes its pager's info line itself, because MudBlazor formats
/// the three counts with a culture of its own and would put an English
/// thousands separator into a German page. That only works while
/// <c>MudDataGridPager_InfoFormat</c> stays a .NET format string with three
/// numeric placeholders — this is the test that says so.
/// </summary>
public sealed class PagerInfoTests
{
    private static readonly CultureInfo German = new("de-DE");

    /// <summary>
    /// Reads the real <c>Strings.resx</c>. A stubbed value would assert nothing
    /// about the file the application actually ships.
    /// </summary>
    private static string InfoFormat()
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .BuildServiceProvider();

        return provider.GetRequiredService<IStringLocalizer<Strings>>()["MudDataGridPager_InfoFormat"];
    }

    public sealed class Compose
    {
        [Fact]
        public void Writes_A_Page_Of_A_Long_List_With_German_Separators()
        {
            var info = PagerInfo.Compose(InfoFormat(), 1, 25, 6327, German);

            Assert.Equal("1–25 von 6.327", info);
        }

        [Fact]
        public void Separates_Every_One_Of_The_Three_Counts()
        {
            var info = PagerInfo.Compose(InfoFormat(), 6301, 6325, 6327, German);

            Assert.Equal("6.301–6.325 von 6.327", info);
        }

        [Fact]
        public void Leaves_A_Short_List_Without_A_Separator()
        {
            var info = PagerInfo.Compose(InfoFormat(), 1, 2, 2, German);

            Assert.Equal("1–2 von 2", info);
        }

        [Fact]
        public void Follows_The_Culture_It_Is_Given()
        {
            var info = PagerInfo.Compose(InfoFormat(), 1, 25, 6327, CultureInfo.GetCultureInfo("en-GB"));

            Assert.Equal("1–25 von 6,327", info);
        }

        [Fact]
        public void Refuses_A_Format_That_Says_Nothing()
        {
            Assert.Throws<ArgumentException>(() => PagerInfo.Compose(" ", 1, 25, 6327, German));
        }
    }

    public sealed class First
    {
        [Fact]
        public void Counts_From_One_On_The_First_Page()
        {
            Assert.Equal(1, PagerInfo.First(skip: 0, onPage: 25));
        }

        [Fact]
        public void Counts_On_From_Where_The_Page_Starts()
        {
            Assert.Equal(51, PagerInfo.First(skip: 50, onPage: 25));
        }

        [Fact]
        public void Is_Nothing_When_The_Page_Carries_Nothing()
        {
            Assert.Equal(0, PagerInfo.First(skip: 50, onPage: 0));
        }
    }

    public sealed class Last
    {
        [Fact]
        public void Is_The_Number_Of_The_Last_Entry_On_The_Page()
        {
            Assert.Equal(25, PagerInfo.Last(skip: 0, onPage: 25));
        }

        [Fact]
        public void Counts_A_Partly_Filled_Last_Page_Correctly()
        {
            Assert.Equal(6327, PagerInfo.Last(skip: 6325, onPage: 2));
        }

        [Fact]
        public void Is_Nothing_When_The_Page_Carries_Nothing()
        {
            Assert.Equal(0, PagerInfo.Last(skip: 50, onPage: 0));
        }
    }

    public sealed class PageCount
    {
        [Fact]
        public void Counts_A_Partly_Filled_Last_Page()
        {
            Assert.Equal(254, PagerInfo.PageCount(total: 6327, pageSize: 25));
        }

        [Fact]
        public void Has_One_Page_For_An_Empty_List()
        {
            Assert.Equal(1, PagerInfo.PageCount(total: 0, pageSize: 25));
        }
    }

    public sealed class Pages
    {
        [Fact]
        public void Offers_Every_Page_Of_A_Short_List()
        {
            Assert.Equal([0, 1, 2], PagerInfo.Pages(page: 0, pageCount: 3));
        }

        [Fact]
        public void Offers_Two_Pages_After_The_First_And_Then_The_Last()
        {
            Assert.Equal([0, 1, 2, null, 253], PagerInfo.Pages(page: 0, pageCount: 254));
        }

        [Fact]
        public void Offers_Two_Pages_On_Either_Side_And_Both_Ends_In_The_Middle()
        {
            Assert.Equal([0, null, 8, 9, 10, 11, 12, null, 253], PagerInfo.Pages(page: 10, pageCount: 254));
        }

        [Fact]
        public void Leaves_No_Gap_Where_No_Page_Is_Left_Out()
        {
            Assert.Equal([0, 1, 2, 3, 4, 5], PagerInfo.Pages(page: 3, pageCount: 6));
        }

        [Fact]
        public void Offers_The_First_And_Two_Pages_Before_The_Last()
        {
            Assert.Equal([0, null, 251, 252, 253], PagerInfo.Pages(page: 253, pageCount: 254));
        }
    }
}
