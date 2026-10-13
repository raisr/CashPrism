using CashPrism.Domain.Contracts;

namespace CashPrism.Domain.Tests.Unit.Contracts;

public sealed class ContractTests
{
    private static Contract Create(
        string interval = "monatlich",
        long amountInCents = -1_200,
        DateTime? newestBookedOn = null,
        string contractId = "0f3c9a1e5b7d4c2a8e6f1b3d5a7c9e0f",
        string currency = "EUR")
        => new(contractId, interval, "Stadtwerke", "Wohnen", "Strom", amountInCents, currency, newestBookedOn ?? new DateTime(2026, 10, 1), false);

    public sealed class Constructor
    {
        [Fact]
        public void Refuses_A_Contract_Without_An_Identifier()
            => Assert.Throws<ArgumentException>(() => Create(contractId: " "));

        [Fact]
        public void Refuses_A_Contract_Without_A_Currency()
            => Assert.Throws<ArgumentException>(() => Create(currency: ""));
    }

    public sealed class IsIncome
    {
        [Fact]
        public void Positive_Amount_Is_Income()
            => Assert.True(Create(amountInCents: 250_000).IsIncome);

        [Fact]
        public void Negative_Amount_Is_Not_Income()
            => Assert.False(Create(amountInCents: -250_000).IsIncome);
    }

    public sealed class MonthlyAmountInCents
    {
        [Theory]
        [InlineData("monatlich", -1_200, -1_200)]
        [InlineData("zweimonatlich", -1_200, -600)]
        [InlineData("vierteljaehrlich", -1_200, -400)]
        [InlineData("halbjaehrlich", -1_200, -200)]
        [InlineData("jaehrlich", -1_200, -100)]
        public void Known_Interval_Is_Spread_Over_Its_Months(string interval, long amountInCents, long expected)
            => Assert.Equal(expected, Create(interval, amountInCents).MonthlyAmountInCents);

        [Fact]
        public void Half_Cent_Is_Rounded_Away_From_Zero()
            => Assert.Equal(-1, Create("halbjaehrlich", -3).MonthlyAmountInCents);

        [Fact]
        public void Unknown_Interval_Has_No_Monthly_Amount()
            => Assert.Null(Create("woechentlich").MonthlyAmountInCents);
    }

    public sealed class IsPresumablyEnded
    {
        private static readonly DateOnly CurrentTo = new(2026, 10, 13);

        [Fact]
        public void Newest_Booking_Exactly_Two_Intervals_Back_Is_Not_Ended()
            => Assert.False(Create("monatlich", newestBookedOn: new DateTime(2026, 8, 13)).IsPresumablyEnded(CurrentTo));

        [Fact]
        public void Newest_Booking_A_Day_More_Than_Two_Intervals_Back_Is_Ended()
            => Assert.True(Create("monatlich", newestBookedOn: new DateTime(2026, 8, 12)).IsPresumablyEnded(CurrentTo));

        [Fact]
        public void Yearly_Contract_Booked_Within_Two_Years_Is_Not_Ended()
            => Assert.False(Create("jaehrlich", newestBookedOn: new DateTime(2024, 10, 13)).IsPresumablyEnded(CurrentTo));

        [Fact]
        public void Yearly_Contract_Booked_More_Than_Two_Years_Back_Is_Ended()
            => Assert.True(Create("jaehrlich", newestBookedOn: new DateTime(2024, 10, 12)).IsPresumablyEnded(CurrentTo));

        [Fact]
        public void Time_Of_Day_On_The_Booking_Does_Not_Move_The_Boundary()
            => Assert.False(Create("monatlich", newestBookedOn: new DateTime(2026, 8, 13, 23, 0, 0)).IsPresumablyEnded(CurrentTo));

        [Fact]
        public void Unknown_Interval_Is_Never_Ended()
            => Assert.False(Create("woechentlich", newestBookedOn: new DateTime(2000, 1, 1)).IsPresumablyEnded(CurrentTo));
    }
}
