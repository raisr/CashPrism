using System.Globalization;
using CashPrism.Web.Bookings;

namespace CashPrism.Web.Tests.Unit.Bookings;

/// <summary>
/// How the booking list writes a figure. The culture is passed in rather than
/// inherited: the host pins German, and a test that read the machine's culture
/// would pass or fail depending on which machine ran it.
/// </summary>
public sealed class BookingFormatTests
{
    private static readonly CultureInfo German = new("de-DE");

    public sealed class Amount
    {
        [Fact]
        public void Spending_Keeps_Its_Minus()
        {
            Assert.Equal("-63,17 EUR", BookingFormat.Amount(-6317, "EUR", German));
        }

        [Fact]
        public void Income_Is_Written_With_A_Plus()
        {
            Assert.Equal("+2.500,00 EUR", BookingFormat.Amount(250_000, "EUR", German));
        }

        [Fact]
        public void Nothing_Is_Neither_Income_Nor_Spending()
        {
            Assert.Equal("0,00 EUR", BookingFormat.Amount(0, "EUR", German));
        }

        [Fact]
        public void A_Whole_Amount_Still_Shows_Both_Decimals()
        {
            Assert.Equal("-12,00 EUR", BookingFormat.Amount(-1200, "EUR", German));
        }

        [Fact]
        public void A_Single_Cent_Is_Not_Rounded_Away()
        {
            Assert.Equal("-0,01 EUR", BookingFormat.Amount(-1, "EUR", German));
        }

        [Fact]
        public void The_Currency_Is_The_One_The_Booking_Carries()
        {
            Assert.EndsWith("CHF", BookingFormat.Amount(-6317, "CHF", German), StringComparison.Ordinal);
        }

        [Fact]
        public void The_Culture_Decides_Where_The_Separators_Go()
        {
            Assert.Equal(
                "+2,500.00 EUR",
                BookingFormat.Amount(250_000, "EUR", CultureInfo.GetCultureInfo("en-GB")));
        }

        [Fact]
        public void The_Amount_And_Its_Currency_Do_Not_Break_Across_Two_Lines()
        {
            Assert.Contains(' ', BookingFormat.Amount(-6317, "EUR", German));
        }

        [Fact]
        public void A_Booking_Without_A_Currency_Is_Not_A_Booking_This_Can_Write()
        {
            Assert.Throws<ArgumentException>(() => BookingFormat.Amount(-6317, " ", German));
        }
    }

    public sealed class Date
    {
        [Fact]
        public void A_Booking_Date_Is_Written_The_German_Way()
        {
            Assert.Equal(
                "12.03.2026",
                BookingFormat.Date(new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Unspecified), German));
        }

        [Fact]
        public void The_Time_Some_Rows_Carry_Is_Left_Out()
        {
            Assert.Equal(
                "12.03.2026",
                BookingFormat.Date(new DateTime(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified), German));
        }
    }
}
