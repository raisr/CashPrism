using Bunit;
using CashPrism.Application.Bookings;
using CashPrism.Web.Bookings;
using CashPrism.Web.Localisation;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// The filter controls. The bar keeps no filter of its own, so every test
/// hands it one and checks the whole filter it reports back.
/// </summary>
public sealed class BookingFilterBarTests
{
    private static readonly DateOnly CurrentTo = new(2026, 10, 2);

    private static readonly BookingPeriod September = new(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

    private static readonly BookingFilterOptions Options = new(
        [new BookingAccount("DE02120300000000202051", "Girokonto"), new BookingAccount("Paynet", "Paynet")],
        [new BookingCategory("Lebensmittel", ["Baeckerei", "Supermarkt"]), new BookingCategory("Mobilitaet", ["Tanken"])]);

    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        private BookingFilter? raised;

        public Render()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddMudServices();
            context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<BookingFilterBar> RenderBar(BookingFilter filter)
            => context.Render<BookingFilterBar>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.Options, Options)
                .Add(p => p.CurrentTo, CurrentTo)
                .Add(p => p.FilterChanged, value => raised = value));

        private static IRenderedComponent<MudSelect<string>> Select(IRenderedComponent<BookingFilterBar> bar, string label)
            => bar.FindComponents<MudSelect<string>>().Single(select => select.Instance.Label == label);

        private static Task PickAsync(IRenderedComponent<BookingFilterBar> bar, string label, string value)
        {
            var select = Select(bar, label);

            return select.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(value));
        }

        [Fact]
        public async Task Raises_The_Picked_Account()
        {
            var bar = RenderBar(BookingFilter.None);

            await PickAsync(bar, "Konto", "Paynet");

            Assert.Equal(new BookingFilter(AccountReference: "Paynet"), raised);
        }

        [Fact]
        public async Task Raises_No_Account_When_Every_Account_Is_Picked()
        {
            var bar = RenderBar(new BookingFilter(AccountReference: "Paynet"));

            await PickAsync(bar, "Konto", string.Empty);

            Assert.Equal(BookingFilter.None, raised);
        }

        [Fact]
        public void Offers_No_Sub_Category_Before_A_Main_Category_Is_Picked()
        {
            var bar = RenderBar(BookingFilter.None);

            Assert.DoesNotContain(bar.FindComponents<MudSelect<string>>(), select => select.Instance.Label == "Unterkategorie");
        }

        [Fact]
        public void Offers_The_Sub_Categories_Of_The_Picked_Main_Category()
        {
            var bar = RenderBar(new BookingFilter(Category: "Lebensmittel"));

            var items = Select(bar, "Unterkategorie").FindComponents<MudSelectItem<string>>().Select(item => item.Instance.Value);
            Assert.Equal(["", "Baeckerei", "Supermarkt"], items);
        }

        [Fact]
        public async Task Drops_The_Sub_Category_When_Another_Main_Category_Is_Picked()
        {
            var bar = RenderBar(new BookingFilter(Category: "Lebensmittel", SubCategory: "Supermarkt"));

            await PickAsync(bar, "Kategorie", "Mobilitaet");

            Assert.Equal(new BookingFilter(Category: "Mobilitaet"), raised);
        }

        [Fact]
        public async Task Turns_A_Period_Preset_Into_Two_Dates_Counted_From_The_Export()
        {
            var bar = RenderBar(BookingFilter.None);

            await PickAsync(bar, "Zeitraum", nameof(BookingPeriodPreset.LastMonth));

            Assert.Equal(new BookingFilter(Period: September), raised);
        }

        [Fact]
        public void Shows_A_Period_That_Matches_A_Preset_As_That_Preset()
        {
            var bar = RenderBar(new BookingFilter(Period: September));

            Assert.Equal(nameof(BookingPeriodPreset.LastMonth), Select(bar, "Zeitraum").Instance.GetState(x => x.Value));
        }

        [Fact]
        public void Shows_Two_Date_Fields_For_A_Period_That_Matches_No_Preset()
        {
            var bar = RenderBar(new BookingFilter(Period: new BookingPeriod(new DateOnly(2026, 9, 3), new DateOnly(2026, 9, 17))));

            Assert.Equal("custom", Select(bar, "Zeitraum").Instance.GetState(x => x.Value));
            Assert.Equal(2, bar.FindComponents<MudDatePicker>().Count);
        }

        [Fact]
        public async Task Shows_Two_Empty_Date_Fields_When_A_Custom_Period_Is_Picked()
        {
            var bar = RenderBar(BookingFilter.None);

            await PickAsync(bar, "Zeitraum", "custom");

            Assert.All(bar.FindComponents<MudDatePicker>(), picker => Assert.Null(picker.Instance.Date));
            Assert.Equal(2, bar.FindComponents<MudDatePicker>().Count);
            Assert.Null(raised);
        }

        [Fact]
        public async Task Starts_The_Date_Fields_At_The_Preset_When_A_Custom_Period_Is_Picked_Over_It()
        {
            var bar = RenderBar(new BookingFilter(Period: September));

            await PickAsync(bar, "Zeitraum", "custom");

            var dates = bar.FindComponents<MudDatePicker>().Select(picker => picker.Instance.Date);
            Assert.Equal([new DateTime(2026, 9, 1), new DateTime(2026, 9, 30)], dates);
            Assert.Null(raised);
        }

        [Fact]
        public async Task Raises_A_Custom_Period_Once_Both_Ends_Are_Picked()
        {
            var bar = RenderBar(BookingFilter.None);
            await PickAsync(bar, "Zeitraum", "custom");
            var pickers = bar.FindComponents<MudDatePicker>();

            await pickers[0].InvokeAsync(() => pickers[0].Instance.DateChanged.InvokeAsync(new DateTime(2026, 9, 3)));
            var afterFirst = raised;
            await pickers[1].InvokeAsync(() => pickers[1].Instance.DateChanged.InvokeAsync(new DateTime(2026, 9, 17)));

            Assert.Null(afterFirst);
            Assert.Equal(
                new BookingFilter(Period: new BookingPeriod(new DateOnly(2026, 9, 3), new DateOnly(2026, 9, 17))),
                raised);
        }

        [Fact]
        public void Raises_The_Picked_Direction()
        {
            var bar = RenderBar(BookingFilter.None);

            bar.FindAll(".cp-seg__opt").Single(button => button.TextContent.Trim() == "Ausgaben").Click();

            Assert.Equal(new BookingFilter(Direction: BookingDirection.Spending), raised);
        }

        [Fact]
        public async Task Raises_Hidden_Transfers_When_The_Switch_Is_Turned_On()
        {
            var bar = RenderBar(BookingFilter.None);
            var toggle = bar.FindComponent<MudSwitch<bool>>();

            await toggle.InvokeAsync(() => toggle.Instance.ValueChanged.InvokeAsync(true));

            Assert.Equal(new BookingFilter(HideTransfers: true), raised);
        }

        [Fact]
        public void Shows_No_Counterparty_Chip_While_No_Counterparty_Filters()
        {
            var bar = RenderBar(BookingFilter.None);

            Assert.Empty(bar.FindAll(".cp-chip"));
        }

        [Fact]
        public void Removes_The_Counterparty_With_Its_Chip()
        {
            var bar = RenderBar(new BookingFilter(Counterparty: "Bäckerei Wendl", HideTransfers: true));

            bar.Find(".cp-chip").Click();

            Assert.Equal(new BookingFilter(HideTransfers: true), raised);
        }

        [Fact]
        public void Names_The_Counterparty_The_Chip_Removes()
        {
            var bar = RenderBar(new BookingFilter(Counterparty: "Bäckerei Wendl"));

            Assert.Equal("Filter „Bäckerei Wendl“ entfernen", bar.Find(".cp-chip").GetAttribute("aria-label"));
        }
    }
}
