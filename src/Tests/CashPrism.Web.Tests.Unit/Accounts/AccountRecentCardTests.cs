using Bunit;
using CashPrism.Application.Accounts;
using CashPrism.Application.Bookings;
using CashPrism.Application.Time;
using CashPrism.Domain.Bookings;
using CashPrism.TestSupport.Bookings;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Accounts;

public sealed class AccountRecentCardTests
{
    private static readonly BookingPeriod Recent = new(new DateOnly(2026, 9, 2), new DateOnly(2026, 10, 1));

    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Render()
        {
            context.Services.AddLocalization();
            context.Services.AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 10, 2, 12, 0, 0, TimeSpan.Zero)));
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        private IRenderedComponent<AccountRecentCard> RenderWith(BookingSummary summary, params Booking[] bookings)
            => context.Render<AccountRecentCard>(parameters => parameters
                .Add(p => p.Flows, new AccountFlows([], Recent, summary, bookings)));

        [Fact]
        public void Names_The_Days_It_Covers()
        {
            var card = RenderWith(new BookingSummary(0, 0, 0));

            Assert.Equal("Letzte 30 Tage", card.Find(".cp-h3").TextContent);
        }

        [Fact]
        public void Shows_What_Came_In_And_What_Went_Out()
        {
            var card = RenderWith(new BookingSummary(7, 250000, 81234));

            Assert.Equal(
                ["2.500,00 €", "812,34 €"],
                card.FindAll(".cp-account-recent__sums .cp-amount").Select(sum => sum.TextContent));
        }

        [Fact]
        public void Lists_The_Bookings_It_Is_Given()
        {
            var card = RenderWith(
                new BookingSummary(2, 0, 0),
                TestBookings.Create(Guid.NewGuid().ToString("N"), new DateTime(2026, 10, 1), counterparty: "Deutsche Bahn"),
                TestBookings.Create(Guid.NewGuid().ToString("N"), new DateTime(2026, 9, 30), counterparty: "Bäckerei"));

            Assert.Equal(["Deutsche Bahn", "Bäckerei"], card.FindAll(".cp-tx__title").Select(title => title.TextContent));
        }

        [Fact]
        public void Says_So_When_The_Account_Has_No_Booking_In_These_Days()
        {
            var card = RenderWith(new BookingSummary(0, 0, 0));

            Assert.Equal("In diesen Tagen hat das Konto keine Buchung.", card.Find(".cp-account-recent__empty").TextContent);
        }

        [Fact]
        public void Raises_The_Booking_That_Was_Picked()
        {
            Booking? picked = null;
            var booking = TestBookings.Create(Guid.NewGuid().ToString("N"), new DateTime(2026, 10, 1));
            var card = context.Render<AccountRecentCard>(parameters => parameters
                .Add(p => p.Flows, new AccountFlows([], Recent, new BookingSummary(1, 0, 6317), [booking]))
                .Add(p => p.OnSelect, (Booking chosen) => picked = chosen));

            card.Find("button.cp-tx").Click();

            Assert.Same(booking, picked);
        }
    }
}
