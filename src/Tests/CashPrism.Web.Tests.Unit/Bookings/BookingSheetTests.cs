using Bunit;
using CashPrism.Application.Time;
using CashPrism.TestSupport.Imports;
using CashPrism.Web.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Web.Tests.Unit.Bookings;

public sealed class BookingSheetTests
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

        private static DateTime Yesterday => Now.LocalDateTime.Date.AddDays(-1);

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Stays_Closed_Without_A_Booking()
        {
            var sheet = context.Render<BookingSheet>();

            Assert.Empty(sheet.FindAll(".cp-sheet"));
        }

        [Fact]
        public void Heads_The_Sheet_With_The_Party_And_The_Day()
        {
            var booking = bookings.Add(Yesterday, counterparty: "Deutsche Bahn", category: "Mobilitaet", subCategory: "Bus & Bahn");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Deutsche Bahn", sheet.Find(".cp-sheet__head .cp-h2").TextContent);
            Assert.Equal("Gestern", sheet.Find(".cp-sheet__subtitle").TextContent);
            Assert.NotNull(sheet.Find(".cp-sheet__head .cp-caticon .icon-car"));
        }

        [Fact]
        public void Shows_The_Amount_Large_With_Its_Cents_Apart()
        {
            var booking = bookings.Add(Yesterday, amountInCents: -5800);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.NotNull(sheet.Find(".cp-amount--lg .cp-amount__cents"));
        }

        [Fact]
        public void Names_The_Other_Party_The_Recipient_Of_Spending()
        {
            var booking = bookings.Add(Yesterday, amountInCents: -5800, counterparty: "Deutsche Bahn");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Contains("Empfänger", sheet.FindAll(".cp-kv dt").Select(term => term.TextContent));
        }

        [Fact]
        public void Names_The_Other_Party_The_Sender_Of_Income()
        {
            var booking = bookings.Add(Yesterday, amountInCents: 402_000, counterparty: "Lindner Logistik GmbH");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Contains("Absender", sheet.FindAll(".cp-kv dt").Select(term => term.TextContent));
        }

        [Fact]
        public void Marks_A_Transfer_Between_The_Own_Accounts()
        {
            var booking = bookings.Add(Yesterday, isTransfer: true);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Zwischen deinen Konten", sheet.Find(".cp-booking-sheet__badges .cp-badge").TextContent);
        }

        [Fact]
        public void Shows_No_Transfer_Badge_On_An_Ordinary_Booking()
        {
            var booking = bookings.Add(Yesterday);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Empty(sheet.FindAll(".cp-booking-sheet__badges"));
        }

        [Fact]
        public void Lists_The_Details_Of_The_Booking()
        {
            var booking = bookings.Add(Yesterday, counterparty: "Deutsche Bahn", category: "Mobilitaet", subCategory: "Bus & Bahn");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            var details = sheet.FindAll(".cp-kv dd").Select(value => value.TextContent);
            Assert.Equal(
                [BookingFormat.Date(Yesterday), "Girokonto", "Mobilität", "Deutsche Bahn", "Kartenzahlung"],
                details);
        }
    }
}
