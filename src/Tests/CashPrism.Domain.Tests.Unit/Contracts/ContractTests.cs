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

    public sealed class NextBookingOn
    {
        private static readonly DateOnly CurrentTo = new(2026, 10, 13);

        [Theory]
        [InlineData("monatlich", 2026, 11, 1)]
        [InlineData("zweimonatlich", 2026, 12, 1)]
        [InlineData("vierteljaehrlich", 2027, 1, 1)]
        [InlineData("halbjaehrlich", 2027, 4, 1)]
        [InlineData("jaehrlich", 2027, 10, 1)]
        public void Known_Interval_Adds_Its_Months_To_The_Newest_Booking(string interval, int year, int month, int day)
            => Assert.Equal(new DateOnly(year, month, day), Create(interval, newestBookedOn: new DateTime(2026, 10, 1)).NextBookingOn(CurrentTo));

        [Fact]
        public void Month_End_Falls_Back_To_The_Last_Day_Of_A_Shorter_Month()
            => Assert.Equal(new DateOnly(2027, 2, 28), Create("monatlich", newestBookedOn: new DateTime(2027, 1, 31)).NextBookingOn(new DateOnly(2027, 2, 1)));

        [Fact]
        public void Time_Of_Day_On_The_Booking_Is_Ignored()
            => Assert.Equal(new DateOnly(2026, 11, 1), Create("monatlich", newestBookedOn: new DateTime(2026, 10, 1, 23, 0, 0)).NextBookingOn(CurrentTo));

        [Fact]
        public void Unknown_Interval_Has_No_Next_Booking()
            => Assert.Null(Create("woechentlich").NextBookingOn(CurrentTo));

        [Fact]
        public void Presumably_Ended_Contract_Has_No_Next_Booking()
            => Assert.Null(Create("monatlich", newestBookedOn: new DateTime(2026, 8, 12)).NextBookingOn(CurrentTo));
    }

    public sealed class DaysOverdue
    {
        private static readonly DateTime NewestBookedOn = new(2026, 9, 1);

        [Fact]
        public void Booking_Expected_In_The_Future_Is_Not_Overdue()
            => Assert.Null(Create(newestBookedOn: NewestBookedOn).DaysOverdue(new DateOnly(2026, 9, 30)));

        [Fact]
        public void Booking_Exactly_Five_Days_Past_Its_Date_Is_Not_Overdue()
            => Assert.Null(Create(newestBookedOn: NewestBookedOn).DaysOverdue(new DateOnly(2026, 10, 6)));

        [Fact]
        public void Booking_Six_Days_Past_Its_Date_Is_Six_Days_Overdue()
            => Assert.Equal(6, Create(newestBookedOn: NewestBookedOn).DaysOverdue(new DateOnly(2026, 10, 7)));

        [Fact]
        public void Unknown_Interval_Is_Never_Overdue()
            => Assert.Null(Create("woechentlich", newestBookedOn: NewestBookedOn).DaysOverdue(new DateOnly(2026, 10, 30)));

        [Fact]
        public void Presumably_Ended_Contract_Is_Not_Overdue()
            => Assert.Null(Create(newestBookedOn: NewestBookedOn).DaysOverdue(new DateOnly(2026, 11, 2)));
    }
}
