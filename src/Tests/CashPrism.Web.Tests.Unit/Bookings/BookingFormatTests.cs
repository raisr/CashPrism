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
        // Spelled out, because both look like their plain cousins in an editor.
        private const string Minus = "−";
        private const string NoBreakSpace = " ";

        [Fact]
        public void Spending_Is_Written_With_A_Real_Minus()
        {
            Assert.Equal($"{Minus}43,18{NoBreakSpace}€", BookingFormat.Amount(-4318, "EUR", German));
        }

        [Fact]
        public void Income_Is_Written_With_A_Plus()
        {
            Assert.Equal($"+250,00{NoBreakSpace}€", BookingFormat.Amount(25_000, "EUR", German));
        }

        [Fact]
        public void Nothing_Is_Written_Without_A_Sign()
        {
            Assert.Equal($"0,00{NoBreakSpace}€", BookingFormat.Amount(0, "EUR", German));
        }

        [Fact]
        public void Another_Currency_Is_Written_As_Its_Code()
        {
            Assert.Equal($"{Minus}4,00{NoBreakSpace}USD", BookingFormat.Amount(-400, "USD", German));
        }

        [Fact]
        public void An_Amount_Above_A_Thousand_Is_Grouped()
        {
            Assert.Equal($"+4.020,00{NoBreakSpace}€", BookingFormat.Amount(402_000, "EUR", German));
        }

        [Fact]
        public void A_Whole_Amount_Still_Shows_Both_Decimals()
        {
            Assert.Equal($"{Minus}12,00{NoBreakSpace}€", BookingFormat.Amount(-1200, "EUR", German));
        }

        [Fact]
        public void A_Single_Cent_Is_Not_Rounded_Away()
        {
            Assert.Equal($"{Minus}0,01{NoBreakSpace}€", BookingFormat.Amount(-1, "EUR", German));
        }

        [Fact]
        public void The_Culture_Decides_Where_The_Separators_Go()
        {
            Assert.Equal(
                $"+2,500.00{NoBreakSpace}€",
                BookingFormat.Amount(250_000, "EUR", CultureInfo.GetCultureInfo("en-GB")));
        }

        /// <summary>
        /// A culture whose own currency symbol is the dollar must not turn a
        /// booking in euros into one, nor a booking in dollars into its symbol.
        /// </summary>
        [Theory]
        [InlineData("EUR", "€")]
        [InlineData("USD", "USD")]
        public void The_Symbol_Comes_From_The_Booking_And_Not_From_The_Culture(string currency, string expected)
        {
            var written = BookingFormat.Amount(-400, currency, CultureInfo.GetCultureInfo("en-US"));

            Assert.EndsWith(NoBreakSpace + expected, written, StringComparison.Ordinal);
        }

        [Fact]
        public void A_Booking_Without_A_Currency_Is_Not_A_Booking_This_Can_Write()
        {
            Assert.Throws<ArgumentException>(() => BookingFormat.Amount(-6317, " ", German));
        }
    }

    public sealed class AmountParts
    {
        [Fact]
        public void Cuts_The_Amount_Where_The_Cents_Begin()
        {
            var (whole, cents) = BookingFormat.AmountParts(-402_018, "EUR", German);

            Assert.Equal("−4.020", whole);
            Assert.Equal(",18 €", cents);
        }

        [Fact]
        public void Cuts_Where_The_Culture_Puts_Its_Decimal_Separator()
        {
            var (whole, cents) = BookingFormat.AmountParts(402_018, "EUR", CultureInfo.GetCultureInfo("en-GB"));

            Assert.Equal("+4,020", whole);
            Assert.Equal(".18 €", cents);
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

    public sealed class DayHeader
    {
        private static readonly DateOnly Today = new(2026, 10, 4);

        [Fact]
        public void Calls_Today_By_Its_Name()
        {
            var header = BookingFormat.DayHeader(new DateTime(2026, 10, 4, 9, 41, 0, DateTimeKind.Unspecified), Today, "Heute", "Gestern", German);

            Assert.Equal("Heute", header);
        }

        [Fact]
        public void Calls_Yesterday_By_Its_Name()
        {
            var header = BookingFormat.DayHeader(new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Unspecified), Today, "Heute", "Gestern", German);

            Assert.Equal("Gestern", header);
        }

        [Fact]
        public void Writes_An_Older_Day_Out_With_Its_Weekday()
        {
            var header = BookingFormat.DayHeader(new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Unspecified), Today, "Heute", "Gestern", German);

            Assert.Equal("Mo, 28. September 2026", header);
        }

        [Fact]
        public void Writes_A_Day_After_Today_Out_As_Well()
        {
            var header = BookingFormat.DayHeader(new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Unspecified), Today, "Heute", "Gestern", German);

            Assert.Equal("Mo, 5. Oktober 2026", header);
        }
    }

    public sealed class Count
    {
        [Fact]
        public void Groups_A_Four_Digit_Count_The_German_Way()
        {
            Assert.Equal("6.327", BookingFormat.Count(6327, German));
        }

        [Fact]
        public void Leaves_A_Small_Count_Alone()
        {
            Assert.Equal("42", BookingFormat.Count(42, German));
        }
    }
}
