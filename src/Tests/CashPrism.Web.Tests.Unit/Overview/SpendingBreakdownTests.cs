using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Localisation;
using CashPrism.Web.Overview;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
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
        public void Draws_The_Ring_Without_A_Tooltip()
        {
            var breakdown = RenderWith(new CategorySpending("Wohnen", 49907));

            Assert.False(breakdown.FindComponent<MudChart<double>>().Instance.ChartOptions?.ShowToolTips);
        }

        [Fact]
        public void Says_So_When_Nothing_Was_Spent()
        {
            var breakdown = RenderWith();

            Assert.Equal("Im September wurde nichts ausgegeben.", breakdown.Find(".cp-breakdown__none").TextContent);
        }
    }
}
