using Bunit;
using CashPrism.Web.Bookings;
using CashPrism.Web.Components;

namespace CashPrism.Web.Tests.Unit.Components;

public sealed class AmountTests
{
    public sealed class Render : IAsyncLifetime
    {
        private readonly BunitContext context = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await context.DisposeAsync();

        [Fact]
        public void Writes_The_Amount_As_The_Booking_Format_Does()
        {
            var amount = context.Render<Amount>(parameters => parameters
                .Add(p => p.AmountInCents, -4318)
                .Add(p => p.Currency, "EUR"));

            Assert.Equal(BookingFormat.Amount(-4318, "EUR"), amount.Find(".cp-amount").TextContent);
        }

        [Fact]
        public void Marks_Income_As_Income()
        {
            var amount = context.Render<Amount>(parameters => parameters
                .Add(p => p.AmountInCents, 402_000)
                .Add(p => p.Currency, "EUR"));

            Assert.True(amount.Find(".cp-amount").ClassList.Contains("cp-amount--in"));
        }

        [Fact]
        public void Marks_Spending_As_Spending()
        {
            var amount = context.Render<Amount>(parameters => parameters
                .Add(p => p.AmountInCents, -4318)
                .Add(p => p.Currency, "EUR"));

            Assert.True(amount.Find(".cp-amount").ClassList.Contains("cp-amount--out"));
        }

        [Fact]
        public void Marks_Nothing_As_Neither()
        {
            var amount = context.Render<Amount>(parameters => parameters
                .Add(p => p.AmountInCents, 0)
                .Add(p => p.Currency, "EUR"));

            Assert.Equal(["cp-amount"], amount.Find(".cp-amount").ClassList);
        }

        [Fact]
        public void Sets_The_Cents_Apart_When_Large()
        {
            var amount = context.Render<Amount>(parameters => parameters
                .Add(p => p.AmountInCents, -402_018)
                .Add(p => p.Currency, "EUR")
                .Add(p => p.Large, true));

            var (whole, cents) = BookingFormat.AmountParts(-402_018, "EUR");
            Assert.Equal(cents, amount.Find(".cp-amount__cents").TextContent);
            Assert.Equal(whole + cents, amount.Find(".cp-amount--lg").TextContent);
        }

        [Fact]
        public void Keeps_The_Cents_In_Line_When_Not_Large()
        {
            var amount = context.Render<Amount>(parameters => parameters
                .Add(p => p.AmountInCents, -402_018)
                .Add(p => p.Currency, "EUR"));

            Assert.Empty(amount.FindAll(".cp-amount__cents"));
        }
    }
}
