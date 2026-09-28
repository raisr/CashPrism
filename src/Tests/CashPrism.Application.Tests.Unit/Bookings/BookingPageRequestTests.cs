using CashPrism.Application.Bookings;
using CashPrism.Application.Paging;

namespace CashPrism.Application.Tests.Unit.Bookings;

/// <summary>
/// The page a booking list asks for. The slice and its guards belong to
/// <see cref="PageRequest"/> and are tested there; what is left here is the
/// ordering, and that the guards still fire through the derived record.
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
        public void Orders_By_The_Column_It_Was_Given()
        {
            var request = new BookingPageRequest(
                Skip: 0,
                Take: 25,
                SortBy: BookingSortColumn.AmountInCents,
                Descending: false);

            Assert.Equal(BookingSortColumn.AmountInCents, request.SortBy);
            Assert.False(request.Descending);
        }

        [Fact]
        public void Refuses_A_Column_There_Is_No_Ordering_For()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BookingPageRequest(Skip: 0, Take: 25, SortBy: (BookingSortColumn)99));
        }

        /// <summary>
        /// The slice is guarded by the base record. This is what fails if someone
        /// ever gives this one a Skip and a Take of its own again.
        /// </summary>
        [Fact]
        public void Still_Refuses_A_Slice_That_Cannot_Be_A_Page()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BookingPageRequest(Skip: -1, Take: 25));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BookingPageRequest(Skip: 0, Take: 0));
        }
    }
}
