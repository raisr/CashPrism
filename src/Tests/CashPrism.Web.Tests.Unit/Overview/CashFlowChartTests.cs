using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Localisation;
using CashPrism.Web.Overview;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Overview;

public sealed class CashFlowChartTests
{
    private static readonly DateOnly October = new(2026, 10, 1);

    private static IReadOnlyList<MonthlyCashFlow> MonthsUpToOctober(int count)
        => Enumerable.Range(0, count)
            .Select(offset => new MonthlyCashFlow(October.AddMonths(offset - count + 1), 400000, 250000))
            .ToList();

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

        private IRenderedComponent<CashFlowChart> RenderWith(int months)
            => context.Render<CashFlowChart>(parameters => parameters
                .Add(p => p.Months, MonthsUpToOctober(months))
                .Add(p => p.CurrentTo, October));

        // The axis itself is only drawn once the browser has measured the
        // chart, which a test renderer never does; the labels handed to it
        // are what this component decides.
        private static string[] Labels(IRenderedComponent<CashFlowChart> chart)
            => chart.FindComponent<MudChart<double>>().Instance.ChartLabels;

        [Fact]
        public void Marks_The_Running_Month_On_The_Axis()
        {
            var chart = RenderWith(3);

            Assert.Equal(["Aug 26", "Sep 26", "Okt 26*"], Labels(chart));
        }

        [Fact]
        public void Explains_The_Running_Month_Under_The_Chart()
        {
            var chart = RenderWith(6);

            Assert.Equal(
                "* Oktober 2026 läuft noch: Der jüngste Export ist vom 01.10.2026.",
                chart.Find(".cp-cashflow__note").TextContent);
        }

        [Fact]
        public void Labels_Every_Other_Month_Beyond_Twelve_Counting_Back_From_The_Running_One()
        {
            var chart = RenderWith(24);

            var labels = Labels(chart);
            Assert.Equal(12, labels.Count(label => label.Length > 0));
            Assert.Equal((string.Empty, "Okt 26*"), (labels[^2], labels[^1]));
        }
    }
}
