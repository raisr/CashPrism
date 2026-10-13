using Bunit;
using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using CashPrism.TestSupport.Contracts;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using ContractsPage = CashPrism.Web.Pages.Contracts;

namespace CashPrism.Web.Tests.Unit.Pages;

/// <summary>
/// The contracts page draws what the use case hands it. Which section a
/// contract belongs to and what a month costs is tested in the use case;
/// here only what a person reads and where a click leads.
/// </summary>
public sealed class ContractsTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 13);

    private static Contract Make(
        string counterparty,
        long amountInCents,
        string interval = "monatlich",
        bool isTransfer = false,
        DateTime? newestBookedOn = null,
        string contractId = "0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f")
        => new(contractId, interval, counterparty, "Wohnen", "Strom", amountInCents, "EUR", newestBookedOn ?? new DateTime(2026, 10, 1), isTransfer);

    public abstract class PageTest : IAsyncLifetime
    {
        protected BunitContext Context { get; } = new();

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await Context.DisposeAsync();

        protected FakeDismissedOverdueStore Dismissals { get; } = new();

        protected IRenderedComponent<ContractsPage> Render(DateOnly? currentTo, params Contract[] contracts)
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddSingleton(new ContractOverviewReader(new ContractForecastReader(new FakeContractReader(currentTo, contracts), Dismissals)));
            Context.Services.AddSingleton(new OverdueDismisser(Dismissals));

            return Context.Render<ContractsPage>();
        }
    }

    public sealed class OnInitializedAsync : PageTest
    {
        [Fact]
        public void Points_At_The_Import_Before_Anything_Was_Imported()
        {
            var page = Render(currentTo: null);

            Assert.Equal("/import", page.Find(".cp-empty a").GetAttribute("href"));
        }

        [Fact]
        public void Says_So_When_The_Export_Carries_No_Contract()
        {
            var page = Render(ExportedOn);

            Assert.Equal("Keine Verträge erkannt", page.Find(".cp-empty h2").TextContent);
        }

        [Fact]
        public void Heads_Each_Interval_Group_With_Its_Name_And_Count()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000), Make("Netzanbieter", -3_999), Make("Versicherung", -12_000, "jaehrlich"));

            Assert.Equal(
                ["Monatlich · 2 Verträge", "Jährlich · 1 Vertrag"],
                page.FindAll(".cp-table__group td:first-child").Select(cell => cell.TextContent));
        }

        [Fact]
        public void Links_A_Contract_To_Its_Bookings()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000));

            Assert.Equal("bookings?contract=0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f", page.Find(".cp-contracts__link").GetAttribute("href"));
        }

        [Fact]
        public void Shows_Income_And_Transfers_In_Sections_Of_Their_Own()
        {
            var page = Render(ExportedOn, Make("Arbeitgeber", 300_000), Make("Tagesgeld", -50_000, isTransfer: true));

            Assert.Equal(
                ["Regelmäßige Einnahmen", "Regelmäßige Umbuchungen"],
                page.FindAll("section h2").Select(heading => heading.TextContent));
        }

        [Fact]
        public void Keeps_The_Presumably_Ended_Contracts_Folded()
        {
            var page = Render(ExportedOn, Make("Altvertrag", -2_000, newestBookedOn: new DateTime(2025, 1, 1)));

            Assert.False(page.Find("details.cp-contracts__ended").HasAttribute("open"));
        }

        [Fact]
        public void Shows_The_Day_The_Next_Booking_Is_Expected()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000, newestBookedOn: new DateTime(2026, 10, 1)));

            Assert.Equal("01.11.2026", page.Find("tbody tr:not(.cp-table__group) td:nth-child(3) .cp-figure").TextContent);
        }

        [Fact]
        public void Marks_An_Overdue_Contract_With_Its_Days()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000, newestBookedOn: new DateTime(2026, 9, 1)));

            Assert.Equal("12 Tage überfällig", page.Find(".cp-contracts__overdue").TextContent.Trim());
        }

        [Fact]
        public void Leaves_The_Expected_Day_Out_For_A_Presumably_Ended_Contract()
        {
            var page = Render(ExportedOn, Make("Altvertrag", -2_000, newestBookedOn: new DateTime(2025, 1, 1)));

            Assert.DoesNotContain("Nächste Buchung", page.FindAll("th").Select(header => header.TextContent));
        }
    }

    public sealed class DismissAsync : PageTest
    {
        [Fact]
        public void Takes_The_Marker_Away()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000, newestBookedOn: new DateTime(2026, 9, 1)));

            page.Find(".cp-contracts__dismiss").Click();

            Assert.Empty(page.FindAll(".cp-contracts__overdue"));
        }

        [Fact]
        public void Stores_The_Dismissal_For_The_Expected_Day()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000, newestBookedOn: new DateTime(2026, 9, 1)));

            page.Find(".cp-contracts__dismiss").Click();

            Assert.Equal(
                [("0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f", new DateOnly(2026, 10, 1))],
                Dismissals.Stored.Select(dismissal => (dismissal.ContractId, dismissal.ExpectedOn)));
        }

        [Fact]
        public void Names_The_Contract_On_The_Button()
        {
            var page = Render(ExportedOn, Make("Stadtwerke", -9_000, newestBookedOn: new DateTime(2026, 9, 1)));

            Assert.Equal("Hinweis für Stadtwerke ausblenden", page.Find(".cp-contracts__dismiss").GetAttribute("aria-label"));
        }
    }
}
