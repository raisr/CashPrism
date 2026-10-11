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

            Assert.Equal("Zwischen deinen Konten", sheet.Find(".cp-booking-sheet__transfer").TextContent);
        }

        [Fact]
        public void Shows_The_Booking_Type_In_Its_German_Spelling()
        {
            var booking = bookings.Add(Yesterday, transactionKind: "Ueberweisung");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Überweisung", sheet.Find(".cp-booking-sheet__kind").TextContent);
        }

        [Fact]
        public void Shows_A_Booking_Type_It_Does_Not_Know_As_The_Export_Wrote_It()
        {
            var booking = bookings.Add(Yesterday, transactionKind: "Scheck");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Scheck", sheet.Find(".cp-booking-sheet__kind").TextContent);
        }

        [Fact]
        public void Puts_The_Booking_Type_Before_The_Transfer_Badge()
        {
            var booking = bookings.Add(Yesterday, isTransfer: true, transactionKind: "Ueberweisung");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            var badges = sheet.FindAll(".cp-booking-sheet__badges .cp-badge").Select(badge => badge.TextContent);
            Assert.Equal(["Überweisung", "Zwischen deinen Konten"], badges);
        }

        [Fact]
        public void Shows_No_Booking_Type_Badge_When_The_Export_Names_None()
        {
            var booking = bookings.Add(Yesterday, isTransfer: true);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Empty(sheet.FindAll(".cp-booking-sheet__kind"));
        }

        [Fact]
        public void Shows_No_Badges_On_An_Ordinary_Booking_Without_A_Type()
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
            // The account's number follows its name in the same value, on a line of its own.
            Assert.Equal(
                [BookingFormat.Date(Yesterday), "Girokonto" + "DE00000000000000000000", "Mobilität", "Deutsche Bahn", "Kartenzahlung"],
                details);
        }

        [Fact]
        public void Shows_The_Own_Account_Number_Under_The_Account_Name()
        {
            var booking = bookings.Add(Yesterday);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("DE00000000000000000000", sheet.Find(".cp-booking-sheet__iban").TextContent);
        }

        [Fact]
        public void Shows_The_Account_Number_Of_The_Other_Party_Under_Its_Name()
        {
            var booking = bookings.Add(Yesterday, counterpartyAccount: "DE89370400440532013000");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            var numbers = sheet.FindAll(".cp-booking-sheet__iban").Select(number => number.TextContent);
            Assert.Equal(["DE00000000000000000000", "DE89370400440532013000"], numbers);
        }

        [Fact]
        public void Shows_The_Direct_Debit_Details_And_The_Tags_When_The_Export_Carries_Them()
        {
            var booking = bookings.Add(
                Yesterday,
                creditorId: "DE98ZZZ09999999999",
                mandateReference: "MR-2024-0042",
                tags: "Urlaub");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            // They follow the payment reference, in this order.
            var terms = sheet.FindAll(".cp-kv dt").Select(term => term.TextContent).TakeLast(3);
            var values = sheet.FindAll(".cp-kv dd").Select(value => value.TextContent).TakeLast(3);
            Assert.Equal(["Gläubiger-ID", "Mandatsreferenz", "Tags"], terms);
            Assert.Equal(["DE98ZZZ09999999999", "MR-2024-0042", "Urlaub"], values);
        }

        [Fact]
        public void Leaves_Out_The_Fields_The_Export_Left_Empty()
        {
            var booking = bookings.Add(Yesterday);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            var terms = sheet.FindAll(".cp-kv dt").Select(term => term.TextContent);
            Assert.DoesNotContain(terms, term => term is "Gläubiger-ID" or "Mandatsreferenz" or "Tags");
            Assert.Empty(sheet.FindAll(".cp-booking-sheet__contract"));
        }

        [Fact]
        public void Marks_A_Contract_With_Its_Interval_In_Its_German_Spelling()
        {
            var booking = bookings.Add(Yesterday, isContract: true, contractInterval: "vierteljaehrlich");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Vertrag · vierteljährlich", sheet.Find(".cp-booking-sheet__contract").TextContent);
        }

        [Fact]
        public void Marks_A_Contract_Without_An_Interval_As_A_Contract_Alone()
        {
            var booking = bookings.Add(Yesterday, isContract: true);

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Vertrag", sheet.Find(".cp-booking-sheet__contract").TextContent);
        }

        [Fact]
        public void Shows_A_Contract_Interval_It_Does_Not_Know_As_The_Export_Wrote_It()
        {
            var booking = bookings.Add(Yesterday, isContract: true, contractInterval: "woechentlich");

            var sheet = context.Render<BookingSheet>(parameters => parameters.Add(p => p.Booking, booking));

            Assert.Equal("Vertrag · woechentlich", sheet.Find(".cp-booking-sheet__contract").TextContent);
        }
    }
}
