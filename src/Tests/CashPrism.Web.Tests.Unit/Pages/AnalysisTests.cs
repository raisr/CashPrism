using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.CashFlow;
using CashPrism.Web.Analysis;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using AnalysisPage = CashPrism.Web.Pages.Analysis;

namespace CashPrism.Web.Tests.Unit.Pages;

/// <summary>
/// The analysis draws what the use case hands it. How the averages and
/// changes are reckoned is tested in the Application and against the
/// database; here only what a person reads on the page.
/// </summary>
public sealed class AnalysisTests
{
    private static readonly DateOnly ExportedOn = new(2026, 10, 1);

    public abstract class PageTest : IAsyncLifetime
    {
        protected BunitContext Context { get; } = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await Context.DisposeAsync();

        protected IRenderedComponent<AnalysisPage> Render(ICashFlowReader reader)
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddSingleton(new SpendingAnalysisReader(reader));

            return Context.Render<AnalysisPage>();
        }

        /// <summary>Renders the page over one category that cost the same every month for two years.</summary>
        protected IRenderedComponent<AnalysisPage> RenderWithSpending()
            => Render(new FakeCashFlowReader(
                ExportedOn,
                Enumerable.Range(1, SpendingAnalysisReader.MonthsRead)
                    .Select(offset => new CategoryNet(ExportedOn.AddMonths(-offset), "Wohnen", -48999))
                    .ToList()));
    }

    public sealed class OnInitializedAsync : PageTest
    {
        [Fact]
        public void Shows_The_Empty_State_When_Nothing_Was_Imported()
        {
            var page = Render(new FakeCashFlowReader(currentTo: null));

            Assert.Equal("Noch nichts importiert.", page.Find(".cp-empty h2").TextContent);
            Assert.Empty(page.FindAll(".cp-seg"));
        }

        [Fact]
        public void Compares_Six_Months_Unless_Another_Period_Is_Picked()
        {
            var page = RenderWithSpending();

            Assert.Equal(6, page.FindComponent<CategoryComparisonTable>().Instance.Period.Months.Count);
        }
    }

    public sealed class PeriodChoice : PageTest
    {
        [Theory]
        [InlineData("3 Monate", 3)]
        [InlineData("12 Monate", 12)]
        public void Redraws_The_Chart_And_The_Table_Over_The_Period_Picked(string choice, int months)
        {
            var page = RenderWithSpending();

            page.FindAll(".cp-seg__opt").Single(option => option.TextContent.Trim() == choice).Click();

            Assert.Equal(
                (months, months),
                (page.FindComponent<SpendingByCategoryChart>().Instance.Period.Months.Count,
                    page.FindComponent<CategoryComparisonTable>().Instance.Period.Months.Count));
        }
    }
}
