using AngleSharp.Dom;
using Bunit;
using CashPrism.Application.Accounts;
using CashPrism.Application.Bookings;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Accounts;
using CashPrism.TestSupport.Bookings;
using CashPrism.TestSupport.CashFlow;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Localisation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Extensions;
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

        protected FakeAccountGroupStore Groups { get; } = new();

        protected FakeBalanceMovementReader Movements { get; } = new();

        /// <summary>The date the stored data is current to; nothing imported when <see langword="null"/>.</summary>
        protected DateOnly? CurrentTo { get; set; } = new(2026, 10, 1);

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
            Context.Services.AddSingleton(new AccountGroupEditor(Balances, Groups));
            Context.Services.AddSingleton<IBookingReader>(Bookings);
            Context.Services.AddSingleton(new AccountFlowReader(Bookings, new FakeCashFlowReader(CurrentTo)));
            Context.Services.AddSingleton(new AccountBalanceHistoryReader(Balances, Movements, new FakeCashFlowReader(CurrentTo)));
            Context.Services.AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 10, 2, 12, 0, 0, TimeSpan.Zero)));
            Context.Services.GetRequiredService<NavigationManager>().NavigateTo(uri);

            return Context.Render<AccountsPage>();
        }

        protected void AddTwoAccounts()
        {
            Balances.Add("Girokonto", 460_373, Giro);
            Balances.Add("Tagesgeld", 2_084_227, Savings);
        }

        /// <summary>Picks the bookings tab of the selected account.</summary>
        protected static void ShowBookings(IRenderedComponent<AccountsPage> page)
            => page.FindAll(".cp-account-tabs [role=tab]")[1].Click();

        /// <summary>The group names as the page shows them, the topmost first.</summary>
        protected static IEnumerable<string> GroupNames(IRenderedComponent<AccountsPage> page)
            => page.FindAll(".cp-account-group__head .cp-label").Select(label => label.TextContent);
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
        public void Starts_With_The_Single_Group_Konten()
        {
            AddTwoAccounts();

            var page = Render();

            Assert.Equal(["Konten"], GroupNames(page));
        }

        [Fact]
        public void Starts_No_Group_Without_An_Account()
        {
            Render();

            Assert.Empty(Groups.Groups);
        }

        [Fact]
        public void Shows_The_Groups_In_Their_Order_Each_With_Its_Sum()
        {
            AddTwoAccounts();
            Groups.AddGroup("Gemeinsam", Giro);
            Groups.AddGroup("Eigene", Savings);

            var page = Render();

            var heads = page.FindAll(".cp-account-group__head").Select(head => (
                head.QuerySelector(".cp-label")!.TextContent,
                head.QuerySelector(".cp-account-group__sum")!.TextContent));
            Assert.Equal([("Gemeinsam", "4.603,73 €"), ("Eigene", "20.842,27 €")], heads);
        }

        [Fact]
        public void Shows_The_Cards_Of_Each_Group_Under_It()
        {
            AddTwoAccounts();
            Groups.AddGroup("Gemeinsam", Giro);
            Groups.AddGroup("Eigene", Savings);

            var page = Render();

            Assert.Equal(
                ["Girokonto", "Tagesgeld"],
                page.FindAll(".cp-account-group .cp-account-card__name").Select(name => name.TextContent));
        }

        [Fact]
        public void Says_So_In_An_Empty_Group()
        {
            AddTwoAccounts();
            Groups.AddGroup("Eigene", Giro, Savings);
            Groups.AddGroup("Gemeinsam");

            var page = Render();

            Assert.Equal("Noch kein Konto in dieser Gruppe.", page.Find(".cp-account-group__empty").TextContent);
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

            // The first request is the list's; the overview asks for its own.
            var request = Bookings.Requests[0];
            Assert.Equal((Giro, 0, AccountsPage.BookingsShown), (request.Filter.AccountReference, request.Skip, request.Take));
        }

        [Fact]
        public void Links_To_The_Booking_List_Of_The_Selected_Account()
        {
            AddTwoAccounts();

            var page = Render($"/accounts?account={Giro}");
            ShowBookings(page);

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

        [Fact]
        public void Selects_The_Overview_When_The_Page_Opens()
        {
            AddTwoAccounts();

            var page = Render();

            var overview = page.FindAll(".cp-account-tabs [role=tab]")[0];
            Assert.Equal(("Überblick", "true"), (overview.TextContent.Trim(), overview.GetAttribute("aria-selected")));
            Assert.NotNull(page.Find(".cp-account-flows"));
            Assert.Empty(page.FindAll(".cp-account-bookings"));
        }

        [Fact]
        public void Shows_The_Bookings_Of_The_Account_When_Their_Tab_Is_Picked()
        {
            Balances.Add("Girokonto", 460_373, Giro);
            Bookings.Add(new DateTime(2026, 10, 1), counterparty: "Deutsche Bahn");

            var page = Render();
            ShowBookings(page);

            Assert.Equal("Deutsche Bahn", page.Find(".cp-account-bookings .cp-tx__title").TextContent);
            Assert.Empty(page.FindAll(".cp-account-flows"));
        }

        [Fact]
        public void Draws_The_Balance_History_Of_The_Selected_Account_In_The_Overview()
        {
            Balances.Add("Girokonto", 100000, Giro);
            Movements.Add(Giro, new DateOnly(2026, 9, 30), -20000, 100000);
            Movements.Add(Giro, new DateOnly(2026, 8, 10), 120000, 120000);

            var page = Render();

            Assert.NotNull(page.Find(".cp-account-balance .cp-account-balance__chart"));
        }

        [Fact]
        public void Explains_Instead_Of_A_Line_When_The_History_Does_Not_Hold()
        {
            Balances.Add("Girokonto", 100000, Giro);
            Movements.Add(Giro, new DateOnly(2026, 9, 30), -20000, 100000);
            Movements.Add(Giro, new DateOnly(2026, 8, 10), 120000, 99999);

            var page = Render();

            Assert.Empty(page.FindAll(".cp-account-balance__chart"));
            Assert.StartsWith("Für dieses Konto lässt sich kein Verlauf berechnen", page.Find(".cp-account-balance__note").TextContent);
        }

        [Fact]
        public void Draws_A_Sparkline_Only_On_The_Card_Of_An_Account_Whose_History_Holds()
        {
            AddTwoAccounts();
            Movements.Add(Giro, new DateOnly(2026, 9, 30), 60373, 460373);
            Movements.Add(Giro, new DateOnly(2026, 8, 10), 400000, 400000);
            Movements.Add(Savings, new DateOnly(2026, 8, 10), 2084227, 1);

            var page = Render();

            var cards = page.FindAll(".cp-account-card").Select(card => (
                card.QuerySelector(".cp-account-card__name")!.TextContent,
                card.QuerySelector(".cp-sparkline") is not null));
            Assert.Equal([("Tagesgeld", false), ("Girokonto", true)], cards);
        }

        [Fact]
        public void Lists_At_Most_Four_Bookings_Of_The_Last_Thirty_Days()
        {
            Balances.Add("Girokonto", 460_373, Giro);
            Bookings.Add(new DateTime(2026, 10, 1));
            Bookings.Add(new DateTime(2026, 9, 30));
            Bookings.Add(new DateTime(2026, 9, 29));
            Bookings.Add(new DateTime(2026, 9, 28));
            Bookings.Add(new DateTime(2026, 9, 27));
            Bookings.Add(new DateTime(2026, 9, 26));

            var page = Render();

            Assert.Equal(4, page.FindAll(".cp-account-recent .cp-tx").Count);
        }
    }

    /// <summary>
    /// Edit mode, through the page and the real use case, so every test sees
    /// what a person sees after the change was stored and read back.
    /// </summary>
    public sealed class ChangeAsync : PageTest
    {
        private Guid own;
        private Guid shared;

        private async Task<IRenderedComponent<AccountsPage>> RenderEditingAsync()
        {
            AddTwoAccounts();
            own = Groups.AddGroup("Eigene", Savings, Giro);
            shared = Groups.AddGroup("Gemeinsam");

            var page = Render();
            var toggle = page.FindComponent<MudSwitch<bool>>();
            await toggle.InvokeAsync(() => toggle.Instance.ValueChanged.InvokeAsync(true));

            return page;
        }

        private static IElement Group(IRenderedComponent<AccountsPage> page, int index)
            => page.FindAll(".cp-group-edit__group")[index];

        private static IElement Account(IRenderedComponent<AccountsPage> page, string name)
            => page.FindAll(".cp-group-edit__account")
                .Single(row => row.QuerySelector(".cp-account-card__name")!.TextContent == name);

        private static IRenderedComponent<MudTextField<string>> NameField(IRenderedComponent<AccountsPage> page, string name)
            => page.FindComponents<MudTextField<string>>().Single(field => field.Instance.GetState(x => x.Value) == name);

        [Fact]
        public async Task Shows_A_Name_Field_Per_Group_When_The_Switch_Is_Turned_On()
        {
            var page = await RenderEditingAsync();

            Assert.Equal(
                ["Eigene", "Gemeinsam"],
                page.FindComponents<MudTextField<string>>()
                    .Where(field => field.Instance.Label == "Name der Gruppe")
                    .Select(field => field.Instance.GetState(x => x.Value)));
        }

        [Fact]
        public async Task Renames_A_Group()
        {
            var page = await RenderEditingAsync();
            var field = NameField(page, "Gemeinsam");

            await field.InvokeAsync(() => field.Instance.ValueChanged.InvokeAsync("Mit Anna"));

            Assert.Equal("Mit Anna", Groups.Groups[1].Name);
        }

        [Fact]
        public async Task Shows_The_Stored_Name_Again_When_A_Blank_One_Is_Entered()
        {
            var page = await RenderEditingAsync();
            var field = NameField(page, "Gemeinsam");

            await field.InvokeAsync(() => field.Instance.ValueChanged.InvokeAsync(" "));

            Assert.Equal("Gemeinsam", Groups.Groups[1].Name);
            Assert.Single(page.FindComponents<MudTextField<string>>(), other => other.Instance.GetState(x => x.Value) == "Gemeinsam");
        }

        [Fact]
        public async Task Moves_A_Group_Down()
        {
            var page = await RenderEditingAsync();

            Group(page, 0).QuerySelector(".cp-group-edit__down")!.Click();

            Assert.Equal([shared, own], Groups.Groups.Select(group => group.Id));
        }

        [Fact]
        public async Task Disables_Moving_The_Topmost_Group_Up()
        {
            var page = await RenderEditingAsync();

            Assert.True(Group(page, 0).QuerySelector(".cp-group-edit__up")!.HasAttribute("disabled"));
        }

        [Fact]
        public async Task Moves_An_Account_Up_In_Its_Group()
        {
            var page = await RenderEditingAsync();

            Account(page, "Girokonto").QuerySelector(".cp-group-edit__up")!.Click();

            Assert.Equal(
                [Giro, Savings],
                Groups.Assignments.OrderBy(assignment => assignment.Position).Select(assignment => assignment.AccountReference));
        }

        [Fact]
        public async Task Assigns_An_Account_To_The_Group_Picked()
        {
            var page = await RenderEditingAsync();
            var select = page.FindComponents<MudSelect<Guid>>()[1];

            await select.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(shared));

            Assert.Equal(shared, Groups.Assignments.Single(assignment => assignment.AccountReference == Giro).GroupId);
        }

        [Fact]
        public async Task Disables_The_Delete_Of_A_Group_That_Holds_An_Account()
        {
            var page = await RenderEditingAsync();

            Assert.True(Group(page, 0).QuerySelector(".cp-group-edit__delete")!.HasAttribute("disabled"));
        }

        [Fact]
        public async Task Says_Why_A_Delete_Is_Disabled()
        {
            var page = await RenderEditingAsync();

            Assert.Equal(
                "Nur eine leere Gruppe lässt sich löschen, und nicht die letzte.",
                Group(page, 0).QuerySelector(".cp-group-edit__delete-wrap")!.GetAttribute("title"));
        }

        [Fact]
        public async Task Deletes_An_Empty_Group()
        {
            var page = await RenderEditingAsync();

            Group(page, 1).QuerySelector(".cp-group-edit__delete")!.Click();

            Assert.Equal([own], Groups.Groups.Select(group => group.Id));
        }

        [Fact]
        public async Task Adds_A_Group_At_The_Bottom()
        {
            var page = await RenderEditingAsync();
            var field = page.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Neue Gruppe");
            await field.InvokeAsync(() => field.Instance.ValueChanged.InvokeAsync("Kinder"));

            page.Find("button.cp-group-edit__add-button").Click();

            // MudButton hands the click on asynchronously, so the group may be
            // stored after Click returns.
            page.WaitForAssertion(() => Assert.Equal(["Eigene", "Gemeinsam", "Kinder"], Groups.Groups.Select(group => group.Name)));
        }
    }
}
