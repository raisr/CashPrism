using Bunit;
using CashPrism.Application.Contracts;
using CashPrism.Domain.Contracts;
using CashPrism.Web.Bookings;
using CashPrism.Web.Localisation;
using CashPrism.Web.Overview;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Overview;

public sealed class UpcomingContractsCardTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 13);

    private static ContractForecast Forecast(string counterparty, DateOnly nextBookingOn, int? daysOverdue = null)
        => new(
            new Contract("0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f", "monatlich", counterparty, "Wohnen", "Strom", -9_000, "EUR", nextBookingOn.AddMonths(-1).ToDateTime(TimeOnly.MinValue), false),
            nextBookingOn,
            daysOverdue);

    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddMudServices();
            context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<UpcomingContractsCard> RenderWith(params ContractForecast[] contracts)
            => context.Render<UpcomingContractsCard>(parameters => parameters
                .Add(p => p.Upcoming, new UpcomingContracts(ExportedOn, contracts)));

        [Fact]
        public void Names_The_Last_Day_It_Looks_Ahead_To()
            => Assert.Equal("Erwartete Vertragsbuchungen bis 12.11.2026", RenderWith().Find(".cp-card__sub").TextContent);

        [Fact]
        public void Says_So_When_Nothing_Is_Expected()
            => Assert.Equal(
                "In den nächsten 30 Tagen ist keine Vertragsbuchung fällig.",
                RenderWith().Find(".cp-upcoming__none").TextContent);

        [Fact]
        public void Lists_Each_Contract_With_Its_Expected_Day_And_Amount()
        {
            var card = RenderWith(Forecast("Stadtwerke", new DateOnly(2026, 10, 20)));

            var cells = card.FindAll("tbody td");
            Assert.Equal(
                ("Stadtwerke", "20.10.2026", BookingFormat.Amount(-9_000, "EUR")),
                (cells[0].QuerySelector(".cp-party__name")!.TextContent, cells[1].TextContent.Trim(), cells[2].TextContent.Trim()));
        }

        [Fact]
        public void Marks_An_Overdue_Contract_With_Its_Days()
        {
            var card = RenderWith(Forecast("Stadtwerke", new DateOnly(2026, 10, 1), daysOverdue: 12));

            Assert.Equal("12 Tage überfällig", card.Find(".cp-upcoming__overdue").TextContent);
        }

        [Fact]
        public void Links_To_The_Contracts_Page()
            => Assert.Equal("/contracts", RenderWith().Find(".cp-card__head a").GetAttribute("href"));

        [Fact]
        public void Offers_No_Way_To_Dismiss_A_Marker()
            => Assert.Empty(RenderWith(Forecast("Stadtwerke", new DateOnly(2026, 10, 1), daysOverdue: 12)).FindAll("tbody button"));
    }
}
