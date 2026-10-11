using Bunit;
using CashPrism.Application.Time;
using CashPrism.Domain.Bookings;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Bookings;

public sealed class BookingDayListTests
{
    public sealed class Render : IAsyncLifetime
    {
        // Noon, so the local day is the same on every machine that runs this.
        private static readonly DateTimeOffset Now = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);

        private readonly BunitContext context = new();
        private readonly FakeBookingReader bookings = new();

        public Render()
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            context.Services.AddLocalization();
            context.Services.AddSingleton<IClock>(new FixedClock(Now));
        }

        private static DateTime Today => Now.LocalDateTime.Date;

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Heads_Each_Day_Once()
        {
            Booking[] list =
            [
                bookings.Add(Today, counterparty: "Bäckerei"),
                bookings.Add(Today, counterparty: "Tankstelle"),
                bookings.Add(Today.AddDays(-1), counterparty: "Deutsche Bahn"),
            ];

            var rendered = context.Render<BookingDayList>(parameters => parameters.Add(p => p.Bookings, list));

            Assert.Equal(["Heute", "Gestern"], rendered.FindAll(".cp-txlist__day").Select(day => day.TextContent));
        }

        [Fact]
        public void Shows_The_Party_The_Payment_Reference_And_The_Amount()
        {
            Booking[] list = [bookings.Add(Today, amountInCents: -5800, counterparty: "Deutsche Bahn")];

            var rendered = context.Render<BookingDayList>(parameters => parameters.Add(p => p.Bookings, list));

            Assert.Equal(
                ("Deutsche Bahn", "Kartenzahlung", "−58,00 €"),
                (rendered.Find(".cp-tx__title").TextContent, rendered.Find(".cp-tx__meta").TextContent, rendered.Find(".cp-amount").TextContent));
        }

        [Fact]
        public void Hands_The_Picked_Booking_To_The_Caller()
        {
            Booking[] list = [bookings.Add(Today, counterparty: "Bäckerei"), bookings.Add(Today, counterparty: "Tankstelle")];
            Booking? picked = null;

            var rendered = context.Render<BookingDayList>(parameters => parameters
                .Add(p => p.Bookings, list)
                .Add(p => p.OnSelect, booking => picked = booking));
            rendered.FindAll("button.cp-tx")[1].Click();

            Assert.Same(list[1], picked);
        }
    }
}
