using CashPrism.Application.Bookings;

namespace CashPrism.Application.Tests.Unit.Bookings;

public sealed class BookingFilterTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Narrows_Nothing_When_Nothing_Is_Set()
        {
            var filter = new BookingFilter();

            Assert.Equal(BookingFilter.None, filter);
        }

        [Fact]
        public void Takes_A_Sub_Category_Together_With_Its_Main_Category()
        {
            var filter = new BookingFilter(Category: "Lebensmittel", SubCategory: "Supermarkt");

            Assert.Equal("Supermarkt", filter.SubCategory);
        }

        [Fact]
        public void Refuses_A_Sub_Category_Without_Its_Main_Category()
            => Assert.Throws<ArgumentException>(() => new BookingFilter(SubCategory: "Supermarkt"));

        [Fact]
        public void Refuses_A_Direction_That_Does_Not_Exist()
            => Assert.Throws<ArgumentOutOfRangeException>(() => new BookingFilter(Direction: (BookingDirection)99));
    }
}
