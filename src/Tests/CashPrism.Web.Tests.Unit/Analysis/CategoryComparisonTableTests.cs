using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.Web.Analysis;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Analysis;

public sealed class CategoryComparisonTableTests
{
    private static readonly IReadOnlyList<DateOnly> ThreeMonths =
        [new(2026, 7, 1), new(2026, 8, 1), new(2026, 9, 1)];

    private static CategoryComparison Category(
        string name,
        long averageInCents,
        decimal share = 0.5m,
        decimal? change = 0m,
        int rank = 0)
        => new(name, rank, [averageInCents, averageInCents, averageInCents], averageInCents, share, change);

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

        private IRenderedComponent<CategoryComparisonTable> RenderWith(params CategoryComparison[] categories)
            => context.Render<CategoryComparisonTable>(parameters => parameters
                .Add(p => p.Period, new SpendingPeriod(ThreeMonths, categories.Sum(category => category.AverageInCents), categories)));

        private static IReadOnlyList<string> Column(IRenderedComponent<CategoryComparisonTable> table, int column)
            => table.FindAll("tbody tr").Select(row => row.Children[column].TextContent.Trim()).ToList();

        [Fact]
        public void Lists_Every_Category_In_The_Order_It_Is_Given()
        {
            var table = RenderWith(Category("Wohnen", 127000), Category("Mobilitaet", 19090));

            Assert.Equal(["Wohnen", "Mobilität"], Column(table, 0));
        }

        [Fact]
        public void Shows_The_Share_In_Whole_Per_Cent()
        {
            var table = RenderWith(Category("Wohnen", 127000, share: 0.4475m));

            Assert.Equal("45 %", table.Find(".cp-comparison__share-figure").TextContent);
        }

        [Fact]
        public void Shows_The_Average_Per_Month_In_Whole_Euros()
        {
            var table = RenderWith(Category("Mobilitaet", 19090));

            Assert.Equal(["191 €"], Column(table, 4));
        }

        [Fact]
        public void Shows_The_Change_Against_The_Period_Before()
        {
            var table = RenderWith(Category("Mobilitaet", 19090, change: 0.6978m));

            Assert.Equal(["+70 %"], Column(table, 3));
        }

        [Fact]
        public void Marks_Spending_That_Grew_Noticeably_As_Bad_News()
        {
            var table = RenderWith(Category("Mobilitaet", 19090, change: 0.6978m));

            Assert.Contains("cp-badge--danger", table.Find("tbody td:nth-child(4) .cp-badge").ClassList);
        }

        [Fact]
        public void Marks_Spending_That_Shrank_Noticeably_As_Good_News()
        {
            var table = RenderWith(Category("Lifestyle", 19095, change: -0.2139m));

            Assert.Contains("cp-badge--success", table.Find("tbody td:nth-child(4) .cp-badge").ClassList);
        }

        [Fact]
        public void Leaves_A_Small_Change_Neutral()
        {
            var table = RenderWith(Category("Wohnen", 127000, change: 0.04m));

            Assert.Equal(["cp-badge"], table.Find("tbody td:nth-child(4) .cp-badge").ClassList);
        }

        [Fact]
        public void Shows_No_Arrow_Beside_A_Change_That_Rounds_To_Zero()
        {
            var table = RenderWith(Category("Wohnen", 127000, change: -0.004m));

            Assert.Empty(table.FindAll("tbody td:nth-child(4) .cp-badge i"));
        }

        [Fact]
        public void Points_The_Arrow_Up_When_Spending_Grew()
        {
            var table = RenderWith(Category("Mobilitaet", 19090, change: 0.6978m));

            Assert.Contains("icon-trending-up", table.Find("tbody td:nth-child(4) .cp-badge i").ClassList);
        }

        [Fact]
        public void Calls_A_Category_New_That_Cost_Nothing_In_The_Period_Before()
        {
            var table = RenderWith(Category("Haustiere", 2581, change: null));

            Assert.Equal(["neu"], Column(table, 3));
        }

        [Fact]
        public void Colours_A_Row_By_The_Rank_Of_Its_Category()
        {
            var table = RenderWith(Category("Wohnen", 127000, rank: 1));

            Assert.Contains("--c:var(--cp-prism-blue)", table.Find(".cp-progress__bar").GetAttribute("style"));
        }
    }
}
