using Bunit;
using Bunit.Rendering;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Localisation;
using CashPrism.Web.Overview;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Charts;
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

            var labels = Labels(chart).Select(Visible).ToArray();
            Assert.Equal(12, labels.Count(label => label.Length > 0));
            Assert.Equal((string.Empty, "Okt 26*"), (labels[^2], labels[^1]));
        }

        [Fact]
        public void Keeps_Every_Blank_Label_Apart_From_The_Others()
        {
            var chart = RenderWith(24);

            Assert.Equal(24, Labels(chart).Distinct().Count());
        }

        // What a person sees of a label: the zero-width spaces that keep the
        // blank ones apart show nothing.
        private static string Visible(string label) => label.Replace("\u200B", string.Empty, StringComparison.Ordinal);

        /// <summary>
        /// Renders the box MudChart would show over one point. The pointer
        /// itself is the browser's; which point it is over is what MudChart
        /// hands the template: the series and the axis label of the month.
        /// </summary>
        private IRenderedComponent<ContainerFragment> TipOver(IRenderedComponent<CashFlowChart> chart, int series, int month)
            => context.Render(chart.FindComponent<MudChart<double>>().Instance.TooltipTemplate!(
                (new SvgPath { Index = series, LabelXValue = Labels(chart)[month] }, "var(--cp-income)")));

        [Fact]
        public void Names_The_Series_And_The_Month_Of_The_Point_Under_The_Pointer()
        {
            var chart = RenderWith(3);

            var tip = TipOver(chart, 1, 1);

            Assert.Equal("Ausgaben · Sep 26", tip.Find(".cp-chart-tip__title").TextContent.Trim());
        }

        [Fact]
        public void Gives_The_Amount_Of_The_Point_In_Whole_Euros()
        {
            var chart = RenderWith(3);

            var tip = TipOver(chart, 0, 1);

            Assert.Equal("4.000 €", tip.Find(".cp-chart-tip__value").TextContent);
        }

        [Fact]
        public void Names_The_Month_Of_A_Point_Whose_Label_Is_Blank()
        {
            var chart = RenderWith(24);

            var tip = TipOver(chart, 0, 22);

            Assert.Equal("Einnahmen · Sep 26", tip.Find(".cp-chart-tip__title").TextContent.Trim());
        }

        [Fact]
        public void Leans_The_Box_Of_The_Running_Month_Inwards()
        {
            var chart = RenderWith(3);

            var tip = TipOver(chart, 0, 2);

            Assert.Contains("cp-chart-tip--end", tip.Find(".cp-chart-tip").ClassList);
        }

        [Fact]
        public void Shows_Nothing_For_A_Point_It_Does_Not_Know()
        {
            var chart = RenderWith(3);

            var tip = context.Render(chart.FindComponent<MudChart<double>>().Instance.TooltipTemplate!(
                (new SvgPath { Index = 0, LabelXValue = "Jan 99" }, "var(--cp-income)")));

            Assert.Empty(tip.FindAll(".cp-chart-tip"));
        }
    }
}
