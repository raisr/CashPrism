using Bunit;
using CashPrism.Web.Imports;
using CashPrism.Web.Layout;
using CashPrism.Web.Tests.Unit.Bookings;
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
}
