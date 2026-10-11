using Bunit;
using Bunit.Rendering;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Localisation;
using CashPrism.Web.Overview;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Charts;
using MudBlazor.Extensions;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Overview;

public sealed class SpendingBreakdownTests
{
    private static readonly DateOnly September = new(2026, 9, 1);

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

        private IRenderedComponent<SpendingBreakdown> RenderWith(params CategorySpending[] categories)
            => context.Render<SpendingBreakdown>(parameters => parameters
                .Add(p => p.Month, September)
                .Add(p => p.Categories, categories));

        private static IReadOnlyList<(string Name, string Amount)> Legend(IRenderedComponent<SpendingBreakdown> breakdown)
            => breakdown.FindAll(".cp-legend__item")
                .Select(item => (item.Children[1].TextContent, item.QuerySelector(".cp-legend__value")!.TextContent))
                .ToList();

        [Fact]
        public void Lists_The_Largest_Five_And_Puts_The_Rest_Together()
        {
            var breakdown = RenderWith(
                new("Essen & Trinken", 76194),
                new("Wohnen", 49907),
                new("Mobilitaet", 31316),
                new("Lifestyle", 29086),
                new("Kinder", 22000),
                new("Sonstiges", 10000),
                new("Drogerie", 9990));

            Assert.Equal(
                [
                    ("Essen & Trinken", "762 €"),
                    ("Wohnen", "499 €"),
                    ("Mobilität", "313 €"),
                    ("Lifestyle", "291 €"),
                    ("Kinder", "220 €"),
                    ("Übrige", "200 €"),
                ],
                Legend(breakdown));
        }

        [Fact]
        public void Adds_No_Rest_When_The_Largest_Cover_Everything()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Kinder", 22000));

            Assert.Equal(["Wohnen", "Kinder"], Legend(breakdown).Select(item => item.Name));
        }

        [Fact]
        public void Shows_The_Month_Total_In_The_Middle_Of_The_Ring()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Kinder", 22000));

            Assert.Equal("719 €", breakdown.Find(".cp-breakdown__total b").TextContent);
        }

        [Fact]
        public void Draws_The_Ring_With_A_Tooltip()
        {
            var breakdown = RenderWith(new CategorySpending("Wohnen", 49907));

            Assert.True(breakdown.FindComponent<MudChart<double>>().Instance.ChartOptions?.ShowToolTips);
        }

        /// <summary>
        /// Renders the box MudChart would show over one slice. The pointer
        /// itself is the browser's; which slice it is over is what MudChart
        /// hands the template.
        /// </summary>
        private IRenderedComponent<ContainerFragment> TipOver(IRenderedComponent<SpendingBreakdown> breakdown, int slice)
            => context.Render(breakdown.FindComponent<MudChart<double>>().Instance.TooltipTemplate!(
                (new SvgPath { Index = slice }, "var(--cp-prism-violet)")));

        [Fact]
        public void Names_The_Category_Of_The_Slice_Under_The_Pointer()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Mobilitaet", 31316));

            var tip = TipOver(breakdown, 1);

            Assert.Equal("Mobilität", tip.Find(".cp-chart-tip__title").TextContent.Trim());
        }

        [Fact]
        public void Gives_What_The_Slice_Cost_In_Whole_Euros()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Mobilitaet", 31316));

            var tip = TipOver(breakdown, 0);

            Assert.Equal("499 €", tip.Find(".cp-chart-tip__value").TextContent);
        }

        [Fact]
        public void Names_The_Rest_Over_Its_Slice()
        {
            var breakdown = RenderWith(
                new("Essen & Trinken", 76194),
                new("Wohnen", 49907),
                new("Mobilitaet", 31316),
                new("Lifestyle", 29086),
                new("Kinder", 22000),
                new("Sonstiges", 10000));

            var tip = TipOver(breakdown, SpendingBreakdown.LargestShown);

            Assert.Equal("Übrige", tip.Find(".cp-chart-tip__title").TextContent.Trim());
        }

        [Fact]
        public void Shows_Nothing_For_A_Slice_It_Does_Not_Know()
        {
            var breakdown = RenderWith(new CategorySpending("Wohnen", 49907));

            var tip = TipOver(breakdown, 3);

            Assert.Empty(tip.FindAll(".cp-chart-tip"));
        }

        private static CategorySpending[] SixCategories() =>
        [
            new("Essen & Trinken", 76194),
            new("Wohnen", 49907),
            new("Mobilitaet", 31316),
            new("Lifestyle", 29086),
            new("Kinder", 22000),
            new("Sonstiges", 10000),
        ];

        private string Address() => context.Services.GetRequiredService<NavigationManager>().Uri;

        [Fact]
        public void Links_A_Category_To_Its_Bookings_Of_The_Month()
        {
            var breakdown = RenderWith(new CategorySpending("Mobilitaet", 31316));

            Assert.Equal(
                "bookings?category=Mobilitaet&from=2026-09-01&to=2026-09-30&transfers=hide",
                breakdown.Find("a.cp-legend__link").GetAttribute("href"));
        }

        [Fact]
        public void Links_The_Rest_Nowhere()
        {
            var breakdown = RenderWith(SixCategories());

            Assert.Equal(
                (SpendingBreakdown.LargestShown, "Übrige"),
                (breakdown.FindAll("a.cp-legend__link").Count, breakdown.FindAll(".cp-legend__item:not(a)").Single().Children[1].TextContent));
        }

        [Fact]
        public async Task Opens_The_Bookings_Of_A_Slice_Clicked()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Mobilitaet", 31316));

            await breakdown.InvokeAsync(() => breakdown.FindComponent<MudChart<double>>().Instance.SelectedIndexChanged.InvokeAsync(1));

            Assert.Equal(
                "http://localhost/bookings?category=Mobilitaet&from=2026-09-01&to=2026-09-30&transfers=hide",
                Address());
        }

        [Fact]
        public void Keeps_No_Slice_Selected_So_A_Click_On_The_Largest_Opens_It_Too()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Mobilitaet", 31316));

            Assert.Equal(-1, breakdown.FindComponent<MudChart<double>>().Instance.GetState(chart => chart.SelectedIndex));
        }

        [Fact]
        public void Marks_The_Ring_When_Its_Last_Slice_Is_The_Rest()
        {
            var breakdown = RenderWith(SixCategories());

            Assert.Contains("cp-breakdown__ring--rest", breakdown.Find(".cp-breakdown__ring").ClassList);
        }

        [Fact]
        public void Leaves_The_Ring_Unmarked_Without_A_Rest()
        {
            var breakdown = RenderWith(new("Wohnen", 49907), new("Kinder", 22000));

            Assert.DoesNotContain("cp-breakdown__ring--rest", breakdown.Find(".cp-breakdown__ring").ClassList);
        }

        [Fact]
        public async Task Stays_On_The_Overview_When_The_Rest_Is_Clicked()
        {
            var breakdown = RenderWith(SixCategories());

            await breakdown.InvokeAsync(() => breakdown.FindComponent<MudChart<double>>().Instance.SelectedIndexChanged.InvokeAsync(SpendingBreakdown.LargestShown));

            Assert.Equal("http://localhost/", Address());
        }

        [Fact]
        public void Says_So_When_Nothing_Was_Spent()
        {
            var breakdown = RenderWith();

            Assert.Equal("Im September wurde nichts ausgegeben.", breakdown.Find(".cp-breakdown__none").TextContent);
        }
    }
}
