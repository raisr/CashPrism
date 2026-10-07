using Bunit;
using CashPrism.Web.Access;
using CashPrism.Web.Imports;
using CashPrism.Web.Layout;
using CashPrism.Web.StoredData;
using CashPrism.Web.Tests.Unit.Bookings;
using CashPrism.Web.Tests.Unit.Imports;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Layout;

public sealed class SidebarTests
{
    public sealed class OnInitializedAsync : IAsyncLifetime
    {
        private readonly BunitContext context = new();
        private readonly FakeBookingReader bookings = new();
        private readonly FakeImportRunReader importRuns = new();

        public OnInitializedAsync() => context.AddSidebarServices(bookings, importRuns);

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Shows_How_Many_Bookings_Are_Stored()
        {
            bookings.Count = 42;

            var sidebar = context.Render<Sidebar>();

            Assert.Equal("42", sidebar.Find(".cp-nav__count").TextContent);
        }

        [Fact]
        public void Shows_No_Count_On_An_Empty_Database()
        {
            var sidebar = context.Render<Sidebar>();

            Assert.Empty(sidebar.FindAll(".cp-nav__count"));
        }

        [Fact]
        public void Says_When_The_Latest_Import_Happened()
        {
            var importedAt = new DateTimeOffset(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
            importRuns.Add(importedAt);

            var sidebar = context.Render<Sidebar>();

            Assert.Equal(
                $"Letzter Import: {ImportRunFormat.ImportedOn(importedAt)}",
                sidebar.Find(".cp-privacy__import").TextContent);
        }

        [Fact]
        public void Says_Nothing_About_An_Import_When_None_Happened()
        {
            var sidebar = context.Render<Sidebar>();

            Assert.Empty(sidebar.FindAll(".cp-privacy__import"));
        }
    }

    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render() => context.AddSidebarServices(new FakeBookingReader(), new FakeImportRunReader());

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Offers_A_Logout_That_Posts_To_The_Logout_Endpoint()
        {
            var sidebar = context.Render<Sidebar>();

            var form = sidebar.Find(".cp-sidebar__logout");
            Assert.Equal(("post", AccessPaths.Logout), (form.GetAttribute("method"), form.GetAttribute("action")));
        }
    }

    public sealed class OnImportCompleted : IAsyncLifetime
    {
        private readonly BunitContext context = new();
        private readonly FakeBookingReader bookings = new();
        private readonly FakeImportRunReader importRuns = new();

        public OnImportCompleted() => context.AddSidebarServices(bookings, importRuns);

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public async Task Shows_The_New_Count_Without_A_Reload()
        {
            var sidebar = context.Render<Sidebar>();
            bookings.Count = 42;

            await context.Services.GetRequiredService<ImportActivity>().RunAsync(ImportAsync);

            sidebar.WaitForAssertion(() => Assert.Equal("42", sidebar.Find(".cp-nav__count").TextContent));
        }

        [Fact]
        public async Task Shows_The_Day_Of_The_New_Import_Without_A_Reload()
        {
            var importedAt = new DateTimeOffset(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
            var sidebar = context.Render<Sidebar>();
            importRuns.Add(importedAt);

            await context.Services.GetRequiredService<ImportActivity>().RunAsync(ImportAsync);

            sidebar.WaitForAssertion(() => Assert.Equal(
                $"Letzter Import: {ImportRunFormat.ImportedOn(importedAt)}",
                sidebar.Find(".cp-privacy__import").TextContent));
        }

        private static Task<ImportFeedbackMessage> ImportAsync()
            => Task.FromResult(new ImportFeedbackMessage(ImportFeedbackSeverity.Success, "fertig", Details: []));
    }

    public sealed class OnStoredDataChanged : IAsyncLifetime
    {
        private readonly BunitContext context = new();
        private readonly FakeBookingReader bookings = new();
        private readonly FakeImportRunReader importRuns = new();

        public OnStoredDataChanged() => context.AddSidebarServices(bookings, importRuns);

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        /// <summary>What deleting all data looks like to the drawer.</summary>
        [Fact]
        public void Drops_The_Count_Once_Nothing_Is_Stored_Without_A_Reload()
        {
            bookings.Count = 42;
            var sidebar = context.Render<Sidebar>();
            bookings.Count = 0;

            context.Services.GetRequiredService<StoredDataChanges>().Notify();

            sidebar.WaitForAssertion(() => Assert.Empty(sidebar.FindAll(".cp-nav__count")));
        }
    }
}
