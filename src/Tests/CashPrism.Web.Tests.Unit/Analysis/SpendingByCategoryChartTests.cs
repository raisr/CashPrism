using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Analysis;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Analysis;

public sealed class SpendingByCategoryChartTests
{
    private static readonly IReadOnlyList<DateOnly> TwoMonths = [new(2026, 8, 1), new(2026, 9, 1)];

    private static CategoryComparison Category(string name, int rank, params long[] months)
        => new(name, rank, months, months.Sum() / months.Length, 0m, null);

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

        private IRenderedComponent<SpendingByCategoryChart> RenderWith(params CategoryComparison[] categories)
            => context.Render<SpendingByCategoryChart>(parameters => parameters
                .Add(p => p.Period, new SpendingPeriod(TwoMonths, 283000, categories)));

        // MudChart draws its bars only once the browser has measured it; the
        // series handed to it are what this component decides.
        private static IEnumerable<(string Name, double[] Data)> Series(IRenderedComponent<SpendingByCategoryChart> chart)
            => chart.FindComponent<MudChart<double>>().Instance.ChartSeries
                .Select(series => (series.Name, ((ChartSeries<double>)series).Data.Values.ToArray()));

        [Fact]
        public void Stacks_The_Categories_In_The_Order_Of_Their_Rank()
        {
            var chart = RenderWith(Category("Lifestyle", 1, 100, 200), Category("Wohnen", 0, 50000, 50000));

            Assert.Equal(["Wohnen", "Lifestyle"], Series(chart).Select(series => series.Name));
        }

        [Fact]
        public void Draws_Each_Month_In_Euros()
        {
            var chart = RenderWith(Category("Wohnen", 0, 48999, 44399));

            Assert.Equal([489.99, 443.99], Series(chart).Single().Data);
        }

        [Fact]
        public void Puts_Every_Category_Ranked_After_The_Distinct_Colours_Together()
        {
            var chart = RenderWith(
                Category("Wohnen", 0, 100, 100),
                Category("Haustiere", RankColour.Distinct, 200, 0),
                Category("Kinder", RankColour.Distinct + 1, 300, 400));

            var rest = Series(chart).Last();
            Assert.Equal("Übrige", rest.Name);
            Assert.Equal([5.0, 4.0], rest.Data);
        }

        [Fact]
        public void Leaves_Out_A_Category_That_Cost_Nothing_In_The_Period()
        {
            var chart = RenderWith(Category("Wohnen", 0, 100, 100), Category("Lifestyle", 1, 0, 0));

            Assert.Equal(["Wohnen"], Series(chart).Select(series => series.Name));
        }

        [Fact]
        public void Gives_The_Average_Per_Month_Above_The_Chart()
        {
            var chart = RenderWith(Category("Wohnen", 0, 100, 100));

            Assert.Equal("Ø 2.830 € pro Monat", chart.Find(".cp-card__sub").TextContent);
        }

        [Fact]
        public void Names_The_Months_On_The_Axis()
        {
            var chart = RenderWith(Category("Wohnen", 0, 100, 100));

            Assert.Equal(["Aug 26", "Sep 26"], chart.FindComponent<MudChart<double>>().Instance.ChartLabels);
        }
    }
}
