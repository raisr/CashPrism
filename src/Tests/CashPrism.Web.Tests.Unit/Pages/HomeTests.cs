using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.CashFlow;
using CashPrism.Web.Localisation;
using CashPrism.Web.Overview;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using HomePage = CashPrism.Web.Pages.Home;

namespace CashPrism.Web.Tests.Unit.Pages;

/// <summary>
/// The overview draws what the use case hands it. Which bookings count and
/// how the months are summed is tested in the Domain, the Application and
/// against the database; here only what a person reads on the page.
/// </summary>
public sealed class HomeTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 1);
    private static readonly DateOnly September = new(2026, 9, 1);
    private static readonly DateOnly August = new(2026, 8, 1);

    public abstract class PageTest : IAsyncLifetime
    {
        protected BunitContext Context { get; } = new();

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await Context.DisposeAsync();

        protected IRenderedComponent<HomePage> Render(ICashFlowReader reader)
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddSingleton(new CashFlowOverviewReader(reader));

            return Context.Render<HomePage>();
        }

        protected IRenderedComponent<HomePage> RenderWith(MonthlyCashFlow september, MonthlyCashFlow august)
            => Render(new FakeCashFlowReader(ExportedOn, [august, september]));

        protected static IReadOnlyList<string> Tiles(IRenderedComponent<HomePage> page)
            => page.FindAll(".cp-stat").Select(tile => tile.TextContent.Trim()).ToList();
    }

    public sealed class OnInitializedAsync : PageTest
    {
        [Fact]
        public void Shows_The_Empty_State_When_Nothing_Was_Imported()
        {
            var page = Render(new FakeCashFlowReader(currentTo: null));

            Assert.Equal("Noch nichts importiert.", page.Find(".cp-empty h2").TextContent);
            Assert.Empty(page.FindAll(".cp-stat"));
        }

        [Fact]
        public void Names_The_Last_Complete_Month_On_Every_Tile()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 400000, 250000), new MonthlyCashFlow(August, 400000, 250000));

            var labels = page.FindAll(".cp-stat__label").Select(label => label.TextContent);
            Assert.Equal(["Einnahmen im September", "Ausgaben im September", "Übrig geblieben im September"], labels);
        }

        [Fact]
        public void Shows_Income_Spending_And_What_Was_Left_To_The_Cent()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 408250, 264363), new MonthlyCashFlow(August, 400000, 250000));

            var values = page.FindAll(".cp-stat__value").Select(value => value.TextContent);
            Assert.Equal(["4.082,50 €", "2.643,63 €", "1.438,87 €"], values);
        }

        [Fact]
        public void Shows_The_Change_Against_The_Month_Before()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 408000, 150000), new MonthlyCashFlow(August, 400000, 200000));

            var deltas = page.FindAll(".cp-stat__delta b").Select(delta => delta.TextContent.Trim());
            Assert.Equal(["+2 %", "−25 %", "+580 €"], deltas);
        }

        [Fact]
        public void Marks_Less_Spending_As_A_Good_Change()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 400000, 150000), new MonthlyCashFlow(August, 400000, 200000));

            Assert.True(page.FindAll(".cp-stat__delta b")[1].ClassList.Contains("cp-delta--good"));
        }

        [Fact]
        public void Leaves_Out_The_Change_Of_Income_When_The_Month_Before_Had_None()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 400000, 150000), new MonthlyCashFlow(August, 0, 200000));

            Assert.Empty(page.FindAll(".cp-stat")[0].QuerySelectorAll(".cp-stat__delta"));
        }

        [Fact]
        public void Says_In_The_Lead_How_Much_More_Came_In_Than_Went_Out()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 408250, 264363), new MonthlyCashFlow(August, 400000, 250000));

            Assert.Equal(
                "So steht es um dein Geld im September 2026. Du hast 1.439 € mehr eingenommen als ausgegeben.",
                page.Find(".cp-pagehead__lead").TextContent);
        }

        [Fact]
        public void Colours_The_Difference_In_The_Lead_Green_When_More_Came_In()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 408250, 264363), new MonthlyCashFlow(August, 400000, 250000));

            var figure = page.Find(".cp-pagehead__lead b");
            Assert.Equal(("1.439 €", true), (figure.TextContent, figure.ClassList.Contains("cp-delta--good")));
        }

        [Fact]
        public void Colours_The_Difference_In_The_Lead_Red_When_More_Went_Out()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 200000, 231200), new MonthlyCashFlow(August, 400000, 250000));

            var figure = page.Find(".cp-pagehead__lead b");
            Assert.Equal(("312 €", true), (figure.TextContent, figure.ClassList.Contains("cp-delta--bad")));
        }

        [Fact]
        public void Points_Each_Tile_The_Way_Its_Figure_Moved()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 408000, 150000), new MonthlyCashFlow(August, 400000, 200000));

            var icons = page.FindAll(".cp-stat > .cp-caticon .cp-icon")
                .Select(icon => icon.ClassList.Single(name => name.StartsWith("icon-", StringComparison.Ordinal)));
            Assert.Equal(["icon-arrow-up-right", "icon-arrow-down-right", "icon-arrow-up-right"], icons);
        }

        [Fact]
        public void Colours_Less_Spending_Green_On_Its_Tile()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 400000, 150000), new MonthlyCashFlow(August, 400000, 200000));

            Assert.StartsWith("--c:var(--cp-income)", page.FindAll(".cp-stat > .cp-caticon")[1].GetAttribute("style"));
        }

        [Fact]
        public void Shows_Twelve_Months_To_Begin_With()
        {
            var page = RenderWith(new MonthlyCashFlow(September, 400000, 250000), new MonthlyCashFlow(August, 400000, 250000));

            Assert.Equal(12, page.FindComponent<CashFlowChart>().Instance.Months.Count);
        }
    }

    public sealed class SpanChoice : PageTest
    {
        [Theory]
        [InlineData("6 Monate", 6)]
        [InlineData("12 Monate", 12)]
        [InlineData("2 Jahre", 24)]
        public void Draws_As_Many_Months_As_Were_Chosen(string choice, int months)
        {
            var page = RenderWith(new MonthlyCashFlow(September, 400000, 250000), new MonthlyCashFlow(August, 400000, 250000));

            page.FindAll(".cp-seg__opt").Single(option => option.TextContent.Trim() == choice).Click();

            Assert.Equal(months, page.FindComponent<CashFlowChart>().Instance.Months.Count);
        }
    }
}
