using System.Globalization;
using CashPrism.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// The booking list writes the pager's info line itself, because MudBlazor
/// formats the three counts with a culture of its own and would put an English
/// thousands separator into a German page. That only works while
/// <c>MudDataGridPager_InfoFormat</c> stays a .NET format string with three
/// numeric placeholders — this is the test that says so.
/// </summary>
public sealed class PagerInfoFormatTests
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

    public sealed class TheFormat
    {
        [Fact]
        public void Writes_A_Page_Of_A_Long_List_With_German_Separators()
        {
            var info = string.Format(German, InfoFormat(), 1, 25, 6327);

            Assert.Equal("1–25 von 6.327", info);
        }

        [Fact]
        public void Separates_Every_One_Of_The_Three_Counts()
        {
            var info = string.Format(German, InfoFormat(), 6301, 6325, 6327);

            Assert.Equal("6.301–6.325 von 6.327", info);
        }

        [Fact]
        public void Leaves_A_Short_List_Without_A_Separator()
        {
            var info = string.Format(German, InfoFormat(), 1, 2, 2);

            Assert.Equal("1–2 von 2", info);
        }
    }
}
