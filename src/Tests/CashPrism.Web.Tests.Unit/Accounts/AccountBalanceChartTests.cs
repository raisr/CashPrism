using Bunit;
using CashPrism.Application.Accounts;
using CashPrism.Web.Accounts;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Accounts;

public sealed class AccountBalanceChartTests
{
    private const string Giro = "DE98999500006166400707";

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

        private IRenderedComponent<AccountBalanceChart> RenderWith(bool holds, params DatedBalance[] points)
            => context.Render<AccountBalanceChart>(parameters => parameters
                .Add(p => p.History, new AccountBalanceHistory(Giro, holds, points)));

        private static LineDisplayType LineDisplay(IRenderedComponent<AccountBalanceChart> chart)
            => ((LineChartOptions)chart.FindComponent<MudChart<double>>().Instance.ChartOptions!).LineDisplayType;

        [Fact]
        public void Draws_The_Balance_At_Each_Point_In_Euros()
        {
            var chart = RenderWith(
                true,
                new DatedBalance(new DateOnly(2026, 8, 31), 120000),
                new DatedBalance(new DateOnly(2026, 9, 30), 98050),
                new DatedBalance(new DateOnly(2026, 10, 1), 100000));

            var drawn = chart.FindComponent<MudChart<double>>().Instance;
            Assert.Equal(["Aug 26", "Sep 26", "Okt 26"], drawn.ChartLabels);
            Assert.Equal([1200d, 980.5d, 1000d], drawn.ChartSeries.Single().Data.Values);
        }

        [Fact]
        public void Fills_Beneath_The_Line_While_No_Balance_Is_Below_Zero()
        {
            var chart = RenderWith(
                true,
                new DatedBalance(new DateOnly(2026, 9, 30), 0),
                new DatedBalance(new DateOnly(2026, 10, 1), 100000));

            Assert.Equal(LineDisplayType.Area, LineDisplay(chart));
        }

        [Fact]
        public void Draws_The_Line_Alone_When_One_Balance_Is_Below_Zero()
        {
            var chart = RenderWith(
                true,
                new DatedBalance(new DateOnly(2026, 9, 30), -1),
                new DatedBalance(new DateOnly(2026, 10, 1), 100000));

            Assert.Equal(LineDisplayType.Line, LineDisplay(chart));
        }

        [Fact]
        public void Explains_Instead_Of_Drawing_When_The_History_Does_Not_Hold()
        {
            var chart = RenderWith(false);

            Assert.Empty(chart.FindComponents<MudChart<double>>());
            Assert.StartsWith("Für dieses Konto lässt sich kein Verlauf berechnen", chart.Find(".cp-account-balance__note").TextContent);
        }

        [Fact]
        public void Explains_Instead_Of_Drawing_A_Single_Point()
        {
            var chart = RenderWith(true, new DatedBalance(new DateOnly(2026, 10, 1), 100000));

            Assert.Empty(chart.FindComponents<MudChart<double>>());
            Assert.Equal("Für einen Verlauf hat dieses Konto noch zu wenige Monate.", chart.Find(".cp-account-balance__note").TextContent);
        }
    }
}
