using Bunit;
using CashPrism.Application.Bookings;
using CashPrism.Application.CashFlow;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Bookings;
using CashPrism.TestSupport.CashFlow;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Localisation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using BookingsPage = CashPrism.Web.Pages.Bookings;

namespace CashPrism.Web.Tests.Unit.Pages;

/// <summary>
/// The booking list renders what the reader hands it and asks the reader for
/// every other order and page — the sorting itself is the database's, and
/// <c>BookingReaderTests</c> covers it there.
/// </summary>
public sealed class BookingsTests
{
    // Noon, so the local day is the same on every machine that runs this.
    private static readonly DateTimeOffset Now = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);

    private const string NoBreakSpace = " ";

    private static DateTime Today => Now.LocalDateTime.Date;

    // The export the list is current to, a few days before today.
    private static readonly DateOnly CurrentTo = new(2026, 10, 2);

    public abstract class PageTest : IAsyncLifetime
    {
        protected PageTest()
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddSingleton<IBookingReader>(Reader);
            Context.Services.AddSingleton<IClock>(new FixedClock(Now));
            Context.Services.AddSingleton<ICashFlowReader>(new FakeCashFlowReader(CurrentTo));
        }

        protected BunitContext Context { get; } = new();

        protected FakeBookingReader Reader { get; } = new();

        protected NavigationManager Navigation => Context.Services.GetRequiredService<NavigationManager>();

        /// <summary>
        /// The query the page last put into the address, its parameters in
        /// ordinal order: where a parameter lands in the URL carries nothing.
        /// </summary>
        protected string Query => string.Join('&', new Uri(Navigation.Uri).Query.TrimStart('?').Split('&').Order(StringComparer.Ordinal));

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await Context.DisposeAsync();
    }

    public sealed class OnInitializedAsync : PageTest
    {
        [Fact]
        public void Shows_The_Empty_State_When_Nothing_Is_Stored()
        {
            var page = Context.Render<BookingsPage>();

            Assert.Equal("Es gibt noch keine Buchungen.", page.Find(".cp-empty h2").TextContent);
            Assert.Empty(page.FindAll("table"));
        }

        [Fact]
        public void Asks_For_The_Newest_Bookings_First()
        {
            Reader.Add(Today);

            Context.Render<BookingsPage>();

            var request = Assert.Single(Reader.Requests);
            Assert.Equal(new BookingPageRequest(0, 25, BookingSortColumn.BookedOn, Descending: true), request);
        }

        [Fact]
        public void Groups_The_Bookings_Under_A_Header_Per_Day()
        {
            Reader.Add(Today);
            Reader.Add(Today);
            Reader.Add(Today.AddDays(-1));
            Reader.Add(Today.AddDays(-6));

            var page = Context.Render<BookingsPage>();

            var headers = page.FindAll(".cp-table__group").Select(row => row.TextContent);
            Assert.Equal(["Heute", "Gestern", "Mo, 28. September 2026"], headers);
        }

        [Fact]
        public void Shows_A_Booking_With_Its_Category_And_Its_Amount()
        {
            Reader.Add(Today, amountInCents: -4318, counterparty: "Bäckerei Wendl");

            var page = Context.Render<BookingsPage>();

            Assert.Equal("Bäckerei Wendl", page.Find(".cp-party__name").TextContent);
            Assert.Equal("Lebensmittel", page.Find(".cp-badge--category").TextContent);
            Assert.NotNull(page.Find(".cp-caticon .icon-shopping-basket"));
            Assert.NotNull(page.Find(".cp-amount--out"));
        }

        [Fact]
        public void Lists_The_Bookings_The_Address_Filters_For()
        {
            Reader.Add(Today);
            Navigation.NavigateTo("/bookings?category=Lebensmittel&direction=spending&sort=amount&page=1");

            Context.Render<BookingsPage>();

            var request = Assert.Single(Reader.Requests);
            Assert.Equal(
                new BookingPageRequest(
                    0,
                    25,
                    BookingSortColumn.AmountInCents,
                    Descending: true,
                    Filter: new BookingFilter(Category: "Lebensmittel", Direction: BookingDirection.Spending)),
                request);
        }

        [Fact]
        public void Sums_Over_The_Filter_The_List_Shows()
        {
            Reader.Add(Today);
            Navigation.NavigateTo("/bookings?transfers=hide");

            Context.Render<BookingsPage>();

            Assert.Equal(new BookingFilter(HideTransfers: true), Assert.Single(Reader.SummaryFilters));
        }

        [Fact]
        public void Shows_Count_Income_Spending_And_Balance_Over_The_Whole_Filter()
        {
            Reader.Add(Today);
            Reader.Summary = new BookingSummary(1234, 250000, 7517);

            var page = Context.Render<BookingsPage>();

            var figures = page.FindAll(".cp-summary > span").Select(figure => figure.TextContent.Trim());
            Assert.Equal(
                ["1.234 Buchungen", $"Einnahmen 2.500,00{NoBreakSpace}€", $"Ausgaben 75,17{NoBreakSpace}€", $"Saldo 2.424,83{NoBreakSpace}€"],
                figures);
        }

        [Fact]
        public void Counts_A_Single_Booking_In_The_Singular()
        {
            Reader.Add(Today);
            Reader.Summary = new BookingSummary(1, 0, 4318);

            var page = Context.Render<BookingsPage>();

            Assert.Equal("1 Buchung", page.Find(".cp-summary__count").TextContent.Trim());
        }

        [Fact]
        public void Says_So_When_The_Filter_Lists_Nothing()
        {
            Reader.Count = 12;
            Navigation.NavigateTo("/bookings?category=Mobilitaet");

            var page = Context.Render<BookingsPage>();

            Assert.Equal("Keine Buchung passt zu diesem Filter.", page.Find(".cp-empty h2").TextContent);
            Assert.Empty(page.FindAll("table"));
            Assert.NotEmpty(page.FindAll(".cp-filters"));
        }

        [Fact]
        public void Shows_The_Last_Page_When_The_Address_Points_Past_It()
        {
            for (var i = 0; i < 30; i++)
            {
                Reader.Add(Today);
            }

            Navigation.NavigateTo("/bookings?page=9");

            var page = Context.Render<BookingsPage>();

            page.WaitForAssertion(() => Assert.Equal(new BookingPageRequest(25, 25), Reader.Requests[^1]));
            Assert.Equal("page=2", Query);
        }
    }

    public sealed class Filter : PageTest
    {
        [Fact]
        public void Puts_A_Changed_Filter_Into_The_Address_And_Lists_By_It()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.FindAll(".cp-seg__opt").Single(button => button.TextContent.Trim() == "Einnahmen").Click();

            Assert.Equal("direction=income", Query);
            page.WaitForAssertion(() => Assert.Equal(new BookingFilter(Direction: BookingDirection.Income), Reader.Requests[^1].Filter));
        }

        [Fact]
        public void Starts_Over_On_The_First_Page_When_The_Filter_Changes()
        {
            Reader.Add(Today);
            Navigation.NavigateTo("/bookings?page=3&size=50");
            var page = Context.Render<BookingsPage>();

            page.FindAll(".cp-seg__opt").Single(button => button.TextContent.Trim() == "Ausgaben").Click();

            Assert.Equal("direction=spending&size=50", Query);
        }

        [Fact]
        public void Follows_The_Address_When_It_Changes_Underneath_The_Page()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            Navigation.NavigateTo("/bookings?account=Paynet");

            page.WaitForAssertion(() => Assert.Equal(new BookingFilter(AccountReference: "Paynet"), Reader.Requests[^1].Filter));
        }
    }

    public sealed class ResetFilter : PageTest
    {
        [Fact]
        public void Takes_Every_Filter_Away_And_Keeps_The_Order()
        {
            Reader.Count = 12;
            Navigation.NavigateTo("/bookings?category=Mobilitaet&transfers=hide&sort=amount");
            var page = Context.Render<BookingsPage>();

            page.Find(".cp-empty button").Click();

            // MudButton hands the click on asynchronously, so the address may
            // change after Click returns; asserting at once failed under load.
            page.WaitForAssertion(() => Assert.Equal("sort=amount", Query));
        }
    }

    public sealed class Sort : PageTest
    {
        [Fact]
        public void Puts_The_Order_Into_The_Address()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:last-child .cp-sort").Click();
            page.Find("th:last-child .cp-sort").Click();

            Assert.Equal("order=asc&sort=amount", Query);
        }

        [Fact]
        public void Asks_For_The_Bookings_By_Name_When_The_Name_Is_Clicked()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:first-child .cp-sort").Click();

            page.WaitForAssertion(() => Assert.Equal(
                new BookingPageRequest(0, 25, BookingSortColumn.Counterparty, Descending: true),
                Reader.Requests[^1]));
        }

        [Fact]
        public void Turns_The_Order_Round_On_A_Second_Click()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:last-child .cp-sort").Click();
            page.Find("th:last-child .cp-sort").Click();

            page.WaitForAssertion(() => Assert.Equal(
                new BookingPageRequest(0, 25, BookingSortColumn.AmountInCents, Descending: false),
                Reader.Requests[^1]));
            Assert.Equal("ascending", page.Find("th:last-child").GetAttribute("aria-sort"));
        }

        [Fact]
        public void Drops_The_Day_Headers_While_A_Column_Sorts()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:last-child .cp-sort").Click();

            Assert.Empty(page.FindAll(".cp-table__group"));
        }

        [Fact]
        public void Gives_The_Day_Headers_Back_On_A_Third_Click()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:last-child .cp-sort").Click();
            page.Find("th:last-child .cp-sort").Click();
            page.Find("th:last-child .cp-sort").Click();

            page.WaitForAssertion(() => Assert.Equal(
                new BookingPageRequest(0, 25, BookingSortColumn.BookedOn, Descending: true),
                Reader.Requests[^1]));
            Assert.NotEmpty(page.FindAll(".cp-table__group"));
        }
    }

    public sealed class Open : PageTest
    {
        [Fact]
        public void Opens_The_Sheet_Of_The_Booking_That_Was_Clicked()
        {
            Reader.Add(Today, counterparty: "Bäckerei Wendl");
            Reader.Add(Today, counterparty: "Deutsche Bahn");
            var page = Context.Render<BookingsPage>();

            page.FindAll("tr.cp-table__row")[1].Click();

            Assert.Equal("Deutsche Bahn", page.Find(".cp-sheet .cp-h2").TextContent);
        }

        [Fact]
        public void Opens_The_Sheet_From_The_Keyboard()
        {
            Reader.Add(Today, counterparty: "Deutsche Bahn");
            var page = Context.Render<BookingsPage>();

            page.Find("tr.cp-table__row").KeyDown("Enter");

            Assert.NotNull(page.Find(".cp-sheet"));
        }
    }

    public sealed class Close : PageTest
    {
        [Fact]
        public void Closes_The_Sheet_On_Escape()
        {
            Reader.Add(Today, counterparty: "Deutsche Bahn");
            var page = Context.Render<BookingsPage>();
            page.Find("tr.cp-table__row").Click();

            page.Find(".cp-sheet").KeyDown("Escape");

            Assert.Empty(page.FindAll(".cp-sheet"));
        }

        [Fact]
        public void Gives_The_Focus_Back_To_The_Row_The_Sheet_Was_Opened_From()
        {
            Reader.Add(Today, counterparty: "Bäckerei Wendl");
            Reader.Add(Today, counterparty: "Deutsche Bahn");
            var page = Context.Render<BookingsPage>();

            // bUnit writes a row's reference into the markup only on the render
            // that creates the row, so it is read before anything re-renders.
            var clicked = page.FindAll("tr.cp-table__row")[1].GetAttribute("blazor:elementReference");
            page.FindAll("tr.cp-table__row")[1].Click();

            page.Find(".cp-sheet").KeyDown("Escape");

            // The sheet took the focus when it opened; the second call is the
            // way back, and it has to land on the row that was clicked.
            page.WaitForAssertion(() => Context.JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2));
            var focus = Context.JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2)[1];
            var focused = Assert.IsType<ElementReference>(focus.Arguments[0]);
            Assert.Equal(clicked, focused.Id);
        }
    }

    public sealed class Paging : PageTest
    {
        [Fact]
        public void Asks_For_The_Page_That_Was_Picked()
        {
            for (var i = 0; i < 60; i++)
            {
                Reader.Add(Today);
            }

            var page = Context.Render<BookingsPage>();

            page.FindAll(".cp-pager__page").Single(button => button.TextContent.Trim() == "3").Click();

            page.WaitForAssertion(() => Assert.Equal(new BookingPageRequest(50, 25), Reader.Requests[^1]));
        }

        [Fact]
        public void Starts_Over_On_The_First_Page_When_The_Page_Size_Changes()
        {
            for (var i = 0; i < 60; i++)
            {
                Reader.Add(Today);
            }

            var page = Context.Render<BookingsPage>();
            page.FindAll(".cp-pager__page").Single(button => button.TextContent.Trim() == "3").Click();

            page.Find(".cp-pager select").Change("50");

            page.WaitForAssertion(() => Assert.Equal(new BookingPageRequest(0, 50), Reader.Requests[^1]));
        }

        [Fact]
        public void Puts_The_Page_Into_The_Address()
        {
            for (var i = 0; i < 60; i++)
            {
                Reader.Add(Today);
            }

            var page = Context.Render<BookingsPage>();

            page.FindAll(".cp-pager__page").Single(button => button.TextContent.Trim() == "3").Click();

            Assert.Equal("page=3", Query);
        }
    }
}
