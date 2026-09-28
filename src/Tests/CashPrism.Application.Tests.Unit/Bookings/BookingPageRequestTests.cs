using CashPrism.Application.Bookings;

namespace CashPrism.Application.Tests.Unit.Bookings;

/// <summary>
/// The page a list asks for. Its values come from a browser, so what cannot be
/// a page is refused here rather than turned into a query.
/// </summary>
public sealed class BookingPageRequestTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Reads_The_Newest_Bookings_First_When_Nothing_Says_Otherwise()
        {
            var request = new BookingPageRequest(Skip: 0, Take: 25);

            Assert.Equal(BookingSortColumn.BookedOn, request.SortBy);
            Assert.True(request.Descending);
        }

        [Fact]
        public void Accepts_A_Page_Beyond_The_First()
        {
            var request = new BookingPageRequest(Skip: 50, Take: 25);

            Assert.Equal(50, request.Skip);
        }

        [Fact]
        public void Refuses_To_Start_Before_The_First_Booking()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BookingPageRequest(Skip: -1, Take: 25));
        }

        [Fact]
        public void Refuses_A_Page_Holding_Nothing()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BookingPageRequest(Skip: 0, Take: 0));
        }

        [Fact]
        public void Refuses_A_Page_Larger_Than_The_Ceiling()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BookingPageRequest(Skip: 0, Take: BookingPageRequest.MaxTake + 1));
        }

        [Fact]
        public void Accepts_A_Page_Of_Exactly_The_Ceiling()
        {
            var request = new BookingPageRequest(Skip: 0, Take: BookingPageRequest.MaxTake);

            Assert.Equal(BookingPageRequest.MaxTake, request.Take);
        }

        [Fact]
        public void Refuses_A_Column_There_Is_No_Ordering_For()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BookingPageRequest(Skip: 0, Take: 25, SortBy: (BookingSortColumn)99));
        }
    }
}
