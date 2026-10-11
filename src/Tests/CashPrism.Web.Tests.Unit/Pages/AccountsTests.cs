using Bunit;
using CashPrism.Application.Accounts;
using CashPrism.Application.Bookings;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Accounts;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Localisation;
using CashPrism.Web.Tests.Unit.Bookings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using AccountsPage = CashPrism.Web.Pages.Accounts;

namespace CashPrism.Web.Tests.Unit.Pages;

/// <summary>
/// The accounts page draws what the use case hands it. How a balance follows
/// from an export and which run is the latest is tested in the Domain and
/// against the database; here only what a person reads and where a click leads.
/// </summary>
public sealed class AccountsTests
{
    private const string Giro = "DE98999500006166400707";
    private const string Savings = "DE33999500006361627181";

    public abstract class PageTest : IAsyncLifetime
    {
        protected BunitContext Context { get; } = new();

        protected FakeAccountBalanceReader Balances { get; } = new();

        protected FakeBookingReader Bookings { get; } = new();

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await Context.DisposeAsync();

        protected IRenderedComponent<AccountsPage> Render(string uri = "/accounts")
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddSingleton(new AccountOverviewReader(Balances));
            Context.Services.AddSingleton<IBookingReader>(Bookings);
            Context.Services.AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 10, 2, 12, 0, 0, TimeSpan.Zero)));
            Context.Services.GetRequiredService<NavigationManager>().NavigateTo(uri);

            return Context.Render<AccountsPage>();
        }

        protected void AddTwoAccounts()
        {
            Balances.Add("Girokonto", 460_373, Giro);
            Balances.Add("Tagesgeld", 2_084_227, Savings);
        }
    }

    public sealed class OnInitializedAsync : PageTest
    {
        [Fact]
        public void Shows_The_Empty_State_Without_A_Balance()
        {
            var page = Render();

            Assert.Equal("Noch keine Konten", page.Find(".cp-empty h2").TextContent);
            Assert.Empty(page.FindAll(".cp-account-card"));
        }

        [Fact]
        public void Points_The_Empty_State_At_The_Import()
        {
            var page = Render();

            Assert.Equal("/import", page.Find(".cp-empty a").GetAttribute("href"));
        }

        [Fact]
        public void Names_The_Total_And_The_Number_Of_Accounts_In_The_Lead()
        {
            AddTwoAccounts();

            var page = Render();

            Assert.Equal("Deine 2 Konten stehen zusammen bei 25.446,00 €.", page.Find(".cp-pagehead__lead").TextContent);
        }

        [Fact]
        public void Shows_A_Card_Per_Account_With_Name_Masked_Number_And_Balance()
        {
            AddTwoAccounts();

            var page = Render();

            var cards = page.FindAll(".cp-account-card").Select(card => (
                card.QuerySelector(".cp-account-card__name")!.TextContent,
                card.QuerySelector(".cp-account-card__number")!.TextContent,
                card.QuerySelector(".cp-account-card__balance")!.TextContent));
            Assert.Equal(
                [("Tagesgeld", "•••• 7181", "20.842,27 €"), ("Girokonto", "•••• 0707", "4.603,73 €")],
                cards);
        }

        [Fact]
        public void Links_Each_Card_To_Its_Own_Address()
        {
            AddTwoAccounts();

            var page = Render();

            Assert.Equal(
                [$"accounts?account={Savings}", $"accounts?account={Giro}"],
                page.FindAll(".cp-account-card").Select(card => card.GetAttribute("href")));
        }
    }

    public sealed class OnParametersSetAsync : PageTest
    {
        [Fact]
        public void Selects_The_Highest_Balance_Without_An_Account_In_The_Address()
        {
            AddTwoAccounts();

            var page = Render();

            Assert.Equal("Tagesgeld", page.Find(".cp-account-card--on .cp-account-card__name").TextContent);
            Assert.Equal("Tagesgeld", page.Find(".cp-account-head h2").TextContent);
        }

        [Fact]
        public void Selects_The_Account_The_Address_Names()
        {
            AddTwoAccounts();

            var page = Render($"/accounts?account={Giro}");

            Assert.Equal("Girokonto", page.Find(".cp-account-head h2").TextContent);
            Assert.Equal("page", page.Find(".cp-account-card--on").GetAttribute("aria-current"));
        }

        [Fact]
        public void Falls_Back_To_The_Highest_Balance_For_An_Unknown_Account()
        {
            AddTwoAccounts();

            var page = Render("/accounts?account=DE00000000000000000000");

            Assert.Equal("Tagesgeld", page.Find(".cp-account-head h2").TextContent);
        }

        [Fact]
        public void Shows_The_Balance_And_The_Latest_Booking_Of_The_Selected_Account()
        {
            Balances.Add("Girokonto", 460_373, Giro, latestBookedOn: new DateTime(2026, 10, 1));

            var page = Render();

            Assert.Equal("4.603,73 €", page.Find(".cp-account-head__balance .cp-amount").TextContent);
            Assert.Contains("letzte Buchung am 01.10.2026", page.Find(".cp-account-head__meta").TextContent);
        }

        [Fact]
        public void Asks_For_The_Latest_Bookings_Of_The_Selected_Account()
        {
            AddTwoAccounts();

            Render($"/accounts?account={Giro}");

            var request = Assert.Single(Bookings.Requests);
            Assert.Equal((Giro, 0, AccountsPage.BookingsShown), (request.Filter.AccountReference, request.Skip, request.Take));
        }

        [Fact]
        public void Links_To_The_Booking_List_Of_The_Selected_Account()
        {
            AddTwoAccounts();

            var page = Render($"/accounts?account={Giro}");

            // Relative, as every link into the booking list is.
            Assert.Equal($"bookings?account={Giro}", page.Find(".cp-account-bookings__all").GetAttribute("href"));
        }

        [Fact]
        public void Opens_The_Sheet_Of_A_Booking_Picked_From_The_List()
        {
            Balances.Add("Girokonto", 460_373, Giro);
            Bookings.Add(new DateTime(2026, 10, 1), counterparty: "Deutsche Bahn");

            var page = Render();
            page.Find(".cp-txlist button.cp-tx").Click();

            Assert.Equal("Deutsche Bahn", page.Find(".cp-sheet__head .cp-h2").TextContent);
        }
    }
}
