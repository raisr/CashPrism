using CashPrism.Application.Bookings;

namespace CashPrism.Application.Tests.Unit.Bookings;

public sealed class BookingPeriodTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Takes_A_Single_Day()
        {
            var day = new DateOnly(2026, 3, 12);

            var period = new BookingPeriod(day, day);

            Assert.Equal((day, day), (period.From, period.To));
        }

        [Fact]
        public void Refuses_A_Period_That_Ends_Before_It_Starts()
            => Assert.Throws<ArgumentOutOfRangeException>(
                () => new BookingPeriod(new DateOnly(2026, 3, 12), new DateOnly(2026, 3, 11)));
    }

    public sealed class For
    {
        // The export date sits in the middle of a month, which is what tells
        // a period counted from it apart from one counted from the 1st.
        private static readonly DateOnly CurrentTo = new(2026, 10, 2);

        [Theory]
        [InlineData(BookingPeriodPreset.ThisMonth, "2026-10-01", "2026-10-31")]
        [InlineData(BookingPeriodPreset.LastMonth, "2026-09-01", "2026-09-30")]
        [InlineData(BookingPeriodPreset.Last12Months, "2025-11-01", "2026-10-31")]
        [InlineData(BookingPeriodPreset.ThisYear, "2026-01-01", "2026-12-31")]
        [InlineData(BookingPeriodPreset.LastYear, "2025-01-01", "2025-12-31")]
        public void Covers_Whole_Calendar_Months_Counted_From_The_Export_Date(
            BookingPeriodPreset preset,
            string from,
            string to)
        {
            var period = BookingPeriod.For(preset, CurrentTo);

            Assert.Equal(new BookingPeriod(DateOnly.Parse(from), DateOnly.Parse(to)), period);
        }

        [Fact]
        public void Ends_Last_Month_On_The_Last_Day_Of_February_When_Exported_On_The_Last_Day_Of_March()
        {
            var period = BookingPeriod.For(BookingPeriodPreset.LastMonth, new DateOnly(2026, 3, 31));

            Assert.Equal(new BookingPeriod(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)), period);
        }

        [Fact]
        public void Reaches_Into_The_Year_Before_For_Last_Month_When_Exported_In_January()
        {
            var period = BookingPeriod.For(BookingPeriodPreset.LastMonth, new DateOnly(2026, 1, 1));

            Assert.Equal(new BookingPeriod(new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 31)), period);
        }

        [Fact]
        public void Refuses_A_Preset_There_Is_No_Period_For()
            => Assert.Throws<ArgumentOutOfRangeException>(
                () => BookingPeriod.For((BookingPeriodPreset)99, CurrentTo));
    }
}
