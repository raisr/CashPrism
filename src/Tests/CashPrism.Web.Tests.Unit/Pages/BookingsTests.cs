using Bunit;
using CashPrism.Application.Bookings;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Localisation;
using CashPrism.Web.Tests.Unit.Bookings;
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

    private static DateTime Today => Now.LocalDateTime.Date;

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
        }

        protected BunitContext Context { get; } = new();

        protected FakeBookingReader Reader { get; } = new();

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
    }

    public sealed class SortAsync : PageTest
    {
        [Fact]
        public void Asks_For_The_Bookings_By_Name_When_The_Name_Is_Clicked()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:first-child .cp-sort").Click();

            Assert.Equal(
                new BookingPageRequest(0, 25, BookingSortColumn.Counterparty, Descending: true),
                Reader.Requests[^1]);
        }

        [Fact]
        public void Turns_The_Order_Round_On_A_Second_Click()
        {
            Reader.Add(Today);
            var page = Context.Render<BookingsPage>();

            page.Find("th:last-child .cp-sort").Click();
            page.Find("th:last-child .cp-sort").Click();

            Assert.Equal(
                new BookingPageRequest(0, 25, BookingSortColumn.AmountInCents, Descending: false),
                Reader.Requests[^1]);
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

            Assert.Equal(
                new BookingPageRequest(0, 25, BookingSortColumn.BookedOn, Descending: true),
                Reader.Requests[^1]);
            Assert.NotEmpty(page.FindAll(".cp-table__group"));
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

            Assert.Equal(new BookingPageRequest(50, 25), Reader.Requests[^1]);
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

            Assert.Equal(new BookingPageRequest(0, 50), Reader.Requests[^1]);
        }
    }
}
