using Bunit;
using CashPrism.Application.CashFlow;
using CashPrism.TestSupport.CashFlow;
using CashPrism.Web.Analysis;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Analysis;

public sealed class CounterpartySpendingListTests
{
    private static readonly IReadOnlyList<DateOnly> ThreeMonths =
        [new(2026, 7, 1), new(2026, 8, 1), new(2026, 9, 1)];

    private static readonly IReadOnlyList<DateOnly> SixMonths =
        [new(2026, 4, 1), new(2026, 5, 1), new(2026, 6, 1), new(2026, 7, 1), new(2026, 8, 1), new(2026, 9, 1)];

    public abstract class TableTest : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        protected TableTest()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddMudServices();
            context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        protected IRenderedComponent<CounterpartySpendingList> RenderWith(
            FakeCashFlowReader reader,
            IReadOnlyList<DateOnly>? months = null)
        {
            context.Services.AddSingleton<ICashFlowReader>(reader);

            return context.Render<CounterpartySpendingList>(parameters => parameters
                .Add(p => p.Months, months ?? ThreeMonths));
        }

        protected IRenderedComponent<CounterpartySpendingList> RenderWith(params CounterpartySpending[] counterparties)
            => RenderWith(new FakeCashFlowReader(null, counterparties: counterparties));

        protected static IReadOnlyList<string> Texts(IRenderedComponent<CounterpartySpendingList> list, string selector)
            => list.FindAll(selector).Select(element => element.TextContent.Trim()).ToList();

        protected static CounterpartySpending[] Many(int count)
            => Enumerable.Range(1, count)
                .Select(rank => new CounterpartySpending($"Händler {rank}", 100000 - rank, 1))
                .ToArray();
    }

    public sealed class OnParametersSetAsync : TableTest
    {
        [Fact]
        public void Asks_For_The_First_And_The_Last_Month_Of_The_Period()
        {
            var reader = new FakeCashFlowReader(null);

            RenderWith(reader);

            Assert.Equal((new DateOnly(2026, 7, 1), new DateOnly(2026, 9, 1)), reader.MonthsAskedFor);
        }

        [Fact]
        public void Asks_Again_When_The_Period_Changes()
        {
            var reader = new FakeCashFlowReader(null);
            var list = RenderWith(reader);

            list.Render(parameters => parameters.Add(p => p.Months, SixMonths));

            Assert.Equal((new DateOnly(2026, 4, 1), new DateOnly(2026, 9, 1)), reader.MonthsAskedFor);
        }
    }

    public sealed class Render : TableTest
    {
        [Fact]
        public void Lists_Every_Counterparty_In_The_Order_It_Is_Given()
        {
            var list = RenderWith(
                new CounterpartySpending("Hausverwaltung Ahorn & Partner", 192000, 6),
                new CounterpartySpending("Frischmarkt", 175177, 29));

            Assert.Equal(["Hausverwaltung Ahorn & Partner", "Frischmarkt"], Texts(list, ".cp-tx__title"));
        }

        [Fact]
        public void Shows_The_Number_Of_Bookings()
        {
            var list = RenderWith(new CounterpartySpending("Frischmarkt", 175177, 29));

            Assert.Equal(["29 Buchungen"], Texts(list, ".cp-tx__meta"));
        }

        [Fact]
        public void Says_Booking_In_The_Singular_For_One()
        {
            var list = RenderWith(new CounterpartySpending("Hanse Assekuranz AG", 31240, 1));

            Assert.Equal(["1 Buchung"], Texts(list, ".cp-tx__meta"));
        }

        [Fact]
        public void Numbers_The_Counterparties_By_Rank()
        {
            var list = RenderWith(
                new CounterpartySpending("Hausverwaltung Ahorn & Partner", 192000, 6),
                new CounterpartySpending("Frischmarkt", 175177, 29));

            Assert.Equal(["1", "2"], Texts(list, ".cp-counterparties__rank"));
        }

        [Fact]
        public void Names_The_Length_Of_The_Period()
        {
            var list = RenderWith(new FakeCashFlowReader(null), SixMonths);

            Assert.Equal("Deine größten Empfänger, 6 Monate", list.Find(".cp-card__sub").TextContent);
        }

        [Fact]
        public void Shows_The_Sum_In_Whole_Euros()
        {
            var list = RenderWith(new CounterpartySpending("Frischmarkt", 175177, 29));

            Assert.Equal(["1.752 €"], Texts(list, ".cp-amount"));
        }

        [Fact]
        public void Links_A_Counterparty_To_Its_Spending_In_The_Booking_List()
        {
            var list = RenderWith(new CounterpartySpending("Hausverwaltung Ahorn & Partner", 192000, 6));

            Assert.Equal(
                "bookings?from=2026-07-01&to=2026-09-30&counterparty=Hausverwaltung%20Ahorn%20%26%20Partner&direction=spending&transfers=hide",
                list.Find("a.cp-tx").GetAttribute("href"));
        }

        [Fact]
        public void Says_So_When_Nothing_Went_Out_In_The_Period()
        {
            var list = RenderWith();

            Assert.Equal("Im Zeitraum ging kein Geld an einen Empfänger.", list.Find(".cp-counterparties__none").TextContent);
        }

        [Fact]
        public void Lists_Only_The_Largest_Until_All_Are_Asked_For()
        {
            var list = RenderWith(Many(CounterpartySpendingList.Shown + 5));

            Assert.Equal(CounterpartySpendingList.Shown, list.FindAll(".cp-tx").Count);
        }

        [Fact]
        public void Lists_Every_Counterparty_Once_All_Are_Asked_For()
        {
            var list = RenderWith(Many(CounterpartySpendingList.Shown + 5));

            list.Find(".cp-counterparties__more button").Click();

            Assert.Equal(CounterpartySpendingList.Shown + 5, list.FindAll(".cp-tx").Count);
        }

        [Fact]
        public void Offers_No_Button_When_Every_Counterparty_Is_Listed_Anyway()
        {
            var list = RenderWith(Many(CounterpartySpendingList.Shown));

            Assert.Empty(list.FindAll(".cp-counterparties__more"));
        }
    }
}
