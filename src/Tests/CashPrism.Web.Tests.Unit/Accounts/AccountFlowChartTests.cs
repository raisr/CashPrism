using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Accounts;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Accounts;

public sealed class AccountFlowChartTests
{
    private static readonly IReadOnlyList<MonthlyCashFlow> TwoMonths =
    [
        new(new DateOnly(2026, 8, 1), 250000, 181234),
        new(new DateOnly(2026, 9, 1), 260050, 0),
    ];

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

        private MudChart<double> Chart()
            => context.Render<AccountFlowChart>(parameters => parameters.Add(p => p.Months, TwoMonths))
                .FindComponent<MudChart<double>>().Instance;

        [Fact]
        public void Labels_Each_Month_Without_The_Year()
        {
            Assert.Equal(["Aug", "Sep"], Chart().ChartLabels);
        }

        [Fact]
        public void Draws_What_Came_In_Beside_What_Went_Out_In_Euros()
        {
            var series = Chart().ChartSeries.Select(line => (line.Name, line.Data.Values.ToArray()));

            Assert.Equal([("Rein", new[] { 2500d, 2600.5d }), ("Raus", new[] { 1812.34d, 0d })], series);
        }

        [Fact]
        public void Names_The_Number_Of_Months_Under_The_Title()
        {
            var card = context.Render<AccountFlowChart>(parameters => parameters.Add(p => p.Months, TwoMonths));

            Assert.Equal("Letzte 2 Monate", card.Find(".cp-card__sub").TextContent);
        }
    }
}
