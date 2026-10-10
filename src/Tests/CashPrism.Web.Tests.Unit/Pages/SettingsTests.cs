using Bunit;
using CashPrism.Application.Bookings;
using CashPrism.Application.Imports;
using CashPrism.Application.Persistence;
using CashPrism.Web.Localisation;
using CashPrism.Web.Network;
using CashPrism.Web.Pages;
using CashPrism.Web.StoredData;
using CashPrism.Web.Tests.Unit.Bookings;
using CashPrism.Web.Tests.Unit.Imports;
using CashPrism.Web.Tests.Unit.Network;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace CashPrism.Web.Tests.Unit.Pages;

public sealed class SettingsTests
{
    private static readonly DateTimeOffset AnImportTime = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    public abstract class PageTest : IAsyncLifetime
    {
        protected PageTest()
        {
            Context.JSInterop.Mode = JSRuntimeMode.Loose;
            Context.Services.AddLocalization();
            Context.Services.AddMudServices();
            Context.Services.AddScoped<MudLocalizer, ResourceMudLocalizer>();
            Context.Services.AddSingleton(Changes);
            Context.Services.AddSingleton<IBookingReader>(Bookings);
            Context.Services.AddSingleton<IImportRunReader>(Runs);
            Context.Services.AddSingleton<IDataEraser>(Eraser);
            Context.Services.AddSingleton<IReachability>(Reachability);

            Eraser.OnErase = () =>
            {
                Bookings.Count = 0;
                Runs.Clear();
            };
        }

        protected BunitContext Context { get; } = new();

        protected FakeBookingReader Bookings { get; } = new();

        protected FakeImportRunReader Runs { get; } = new();

        protected FakeDataEraser Eraser { get; } = new();

        protected StoredDataChanges Changes { get; } = new();

        protected FakeReachability Reachability { get; } = new();

        public Task InitializeAsync() => Task.CompletedTask;

        // Asynchronously, because MudBlazor registers services that only
        // implement IAsyncDisposable.
        public async Task DisposeAsync() => await Context.DisposeAsync();

        /// <summary>Renders the page over three bookings from two imports.</summary>
        protected IRenderedComponent<Settings> RenderWithData()
        {
            Bookings.Count = 3;
            Runs.Add(AnImportTime);
            Runs.Add(AnImportTime.AddDays(1));

            return Context.Render<Settings>();
        }

        protected static void Ask(IRenderedComponent<Settings> page)
            => page.Find("button.cp-settings-erase__start").Click();

        protected static void Confirm(IRenderedComponent<Settings> page)
            => page.Find("button.cp-settings-erase__confirm").Click();
    }

    public sealed class Render : PageTest
    {
        [Fact]
        public void Shows_Where_Another_Device_Reaches_CashPrism()
        {
            Reachability.Current = new Reachability(5099, InContainer: false, ["http://192.168.1.5:5099"]);

            var page = RenderWithData();

            Assert.Equal("http://192.168.1.5:5099", page.Find(".cp-settings-reach__urls li").TextContent);
        }

        [Fact]
        public void Offers_To_Delete_What_Is_Stored()
        {
            var page = RenderWithData();

            Assert.False(page.Find("button.cp-settings-erase__start").HasAttribute("disabled"));
        }

        [Fact]
        public void Offers_Nothing_To_Delete_On_An_Empty_Database()
        {
            var page = Context.Render<Settings>();

            Assert.True(page.Find("button.cp-settings-erase__start").HasAttribute("disabled"));
            Assert.Equal("Es sind keine Daten gespeichert.", page.Find(".cp-settings-erase__hint").TextContent);
        }
    }

    public sealed class Erase : PageTest
    {
        [Fact]
        public void Names_What_Will_Be_Deleted_Before_Deleting_It()
        {
            var page = RenderWithData();

            Ask(page);

            Assert.Equal(
                "Das löscht 3 Buchungen und 2 Importe und lässt sich nicht rückgängig machen.",
                page.Find(".cp-alert--danger .cp-alert__body").TextContent);
        }

        [Fact]
        public void Counts_A_Single_Booking_And_Import_In_The_Singular()
        {
            Bookings.Count = 1;
            Runs.Add(AnImportTime);
            var page = Context.Render<Settings>();

            Ask(page);

            Assert.Equal(
                "Das löscht 1 Buchung und 1 Import und lässt sich nicht rückgängig machen.",
                page.Find(".cp-alert--danger .cp-alert__body").TextContent);
        }

        [Fact]
        public void Deletes_Nothing_On_The_First_Click()
        {
            var page = RenderWithData();

            Ask(page);

            Assert.Equal(0, Eraser.Erasures);
        }

        [Fact]
        public void Deletes_Nothing_When_Cancelled()
        {
            var page = RenderWithData();
            Ask(page);

            page.Find("button.cp-settings-erase__cancel").Click();

            Assert.Equal(0, Eraser.Erasures);
            Assert.Empty(page.FindAll(".cp-alert--danger"));
        }

        [Fact]
        public void Deletes_Everything_When_Confirmed()
        {
            var page = RenderWithData();
            Ask(page);

            Confirm(page);

            Assert.Equal(1, Eraser.Erasures);
        }

        [Fact]
        public void Says_The_Data_Is_Gone_Afterwards()
        {
            var page = RenderWithData();
            Ask(page);

            Confirm(page);

            Assert.Equal("Alle Daten gelöscht", page.Find(".cp-alert--success .cp-alert__title").TextContent);
            Assert.True(page.Find("button.cp-settings-erase__start").HasAttribute("disabled"));
        }

        [Fact]
        public void Tells_Whoever_Shows_The_Stored_Data()
        {
            var page = RenderWithData();
            var told = 0;
            Changes.Changed += () => told++;
            Ask(page);

            Confirm(page);

            Assert.Equal(1, told);
        }

        [Fact]
        public void Locks_The_Page_While_It_Deletes()
        {
            var page = RenderWithData();
            Eraser.Hold();
            Ask(page);

            Confirm(page);

            Assert.NotNull(page.Find(".cp-card--busy .cp-settings-erase__busy"));
            Eraser.Release();
            page.WaitForAssertion(() => Assert.Empty(page.FindAll(".cp-card--busy")));
        }
    }
}
