using CashPrism.Application.Bookings;
using CashPrism.Web.Bookings;

namespace CashPrism.Web.Tests.Unit.Bookings;

public sealed class BookingListQueryTests
{
    private static readonly BookingPeriod September = new(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

    public sealed class Parse
    {
        [Fact]
        public void Opens_The_Default_List_Without_A_Query()
            => Assert.Equal(BookingListQuery.Default, BookingListQuery.Parse(string.Empty));

        [Fact]
        public void Reads_Every_Filter_Criterion()
        {
            var query = BookingListQuery.Parse(
                "?account=DE02120300000000202051&category=Lebensmittel&subcategory=Supermarkt"
                + "&from=2026-09-01&to=2026-09-30&counterparty=B%C3%A4ckerei%20Wendl&direction=spending&transfers=hide");

            Assert.Equal(
                new BookingFilter(
                    AccountReference: "DE02120300000000202051",
                    Category: "Lebensmittel",
                    SubCategory: "Supermarkt",
                    Period: September,
                    Counterparty: "Bäckerei Wendl",
                    Direction: BookingDirection.Spending,
                    HideTransfers: true),
                query.Filter);
        }

        [Fact]
        public void Reads_The_Order_And_The_Page()
        {
            var query = BookingListQuery.Parse("sort=amount&order=asc&page=3&size=50");

            Assert.Equal(
                new BookingListQuery(BookingFilter.None, BookingSortColumn.AmountInCents, false, 2, 50),
                query);
        }

        [Fact]
        public void Sorts_A_Column_Largest_First_Unless_Asked_Otherwise()
        {
            var query = BookingListQuery.Parse("sort=counterparty");

            Assert.Equal((BookingSortColumn.Counterparty, true), (query.SortBy, query.Descending));
        }

        [Theory]
        [InlineData("sort=fingerprint")]
        [InlineData("direction=sideways")]
        [InlineData("page=0")]
        [InlineData("page=-2")]
        [InlineData("page=zwei")]
        [InlineData("page=99999999999")]
        [InlineData("size=7")]
        [InlineData("size=500")]
        [InlineData("transfers=yes")]
        [InlineData("account=")]
        public void Falls_Back_To_The_Default_For_A_Value_It_Cannot_Read(string query)
            => Assert.Equal(BookingListQuery.Default, BookingListQuery.Parse(query));

        [Theory]
        [InlineData("from=2026-09-01")]
        [InlineData("to=2026-09-30")]
        [InlineData("from=2026-09-30&to=2026-09-01")]
        [InlineData("from=01.09.2026&to=30.09.2026")]
        public void Ignores_A_Period_Without_Two_Readable_Ends_In_Order(string query)
            => Assert.Null(BookingListQuery.Parse(query).Filter.Period);

        [Fact]
        public void Ignores_A_Sub_Category_Without_Its_Main_Category()
            => Assert.Equal(BookingFilter.None, BookingListQuery.Parse("subcategory=Supermarkt").Filter);
    }

    public sealed class ToParameters
    {
        [Fact]
        public void Leaves_Every_Parameter_Out_For_The_Default_List()
            => Assert.All(BookingListQuery.Default.ToParameters().Values, Assert.Null);

        [Fact]
        public void Writes_The_Period_As_Two_Dates()
        {
            var query = BookingListQuery.Default with { Filter = new BookingFilter(Period: September) };

            var parameters = query.ToParameters();

            Assert.Equal(("2026-09-01", "2026-09-30"), (parameters["from"], parameters["to"]));
        }

        [Fact]
        public void Numbers_The_Page_From_One()
        {
            var query = BookingListQuery.Default with { PageIndex = 2 };

            Assert.Equal("3", query.ToParameters()["page"]);
        }

        [Fact]
        public void Reads_Back_What_It_Wrote()
        {
            var query = new BookingListQuery(
                new BookingFilter(
                    AccountReference: "Paynet",
                    Category: "Mobilitaet",
                    SubCategory: "Tanken",
                    Period: September,
                    Counterparty: "Tankstelle & Co",
                    Direction: BookingDirection.Income,
                    HideTransfers: true),
                BookingSortColumn.BookedOn,
                Descending: false,
                PageIndex: 4,
                PageSize: 100);

            var written = string.Join(
                '&',
                query.ToParameters()
                    .Where(parameter => parameter.Value is not null)
                    .Select(parameter => $"{parameter.Key}={Uri.EscapeDataString((string)parameter.Value!)}"));

            Assert.Equal(query, BookingListQuery.Parse(written));
        }
    }

    public sealed class ToPageRequest
    {
        [Fact]
        public void Asks_For_The_Page_Under_The_Filter()
        {
            var filter = new BookingFilter(Category: "Mobilitaet");
            var query = new BookingListQuery(filter, BookingSortColumn.AmountInCents, false, 2, 50);

            Assert.Equal(
                new BookingPageRequest(100, 50, BookingSortColumn.AmountInCents, Descending: false, Filter: filter),
                query.ToPageRequest());
        }

        [Fact]
        public void Asks_For_The_Newest_First_While_No_Column_Sorts()
        {
            var query = BookingListQuery.Default with { Descending = false };

            Assert.Equal(new BookingPageRequest(0, 25, BookingSortColumn.BookedOn, Descending: true), query.ToPageRequest());
        }
    }
}
