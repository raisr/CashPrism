using CashPrism.Application.Paging;

namespace CashPrism.Application.Tests.Unit.Paging;

/// <summary>
/// The slice a list asks for. Its values come from a browser, so what cannot be
/// a page is refused here rather than turned into a query.
/// </summary>
public sealed class PageRequestTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Takes_The_Slice_It_Was_Given()
        {
            var request = new PageRequest(Skip: 50, Take: 25);

            Assert.Equal(50, request.Skip);
            Assert.Equal(25, request.Take);
        }

        [Fact]
        public void Accepts_The_First_Page()
        {
            var request = new PageRequest(Skip: 0, Take: 25);

            Assert.Equal(0, request.Skip);
        }

        [Fact]
        public void Refuses_To_Start_Before_The_First_Entry()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PageRequest(Skip: -1, Take: 25));
        }

        [Fact]
        public void Refuses_A_Page_Holding_Nothing()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PageRequest(Skip: 0, Take: 0));
        }

        [Fact]
        public void Refuses_A_Page_Larger_Than_The_Ceiling()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PageRequest(Skip: 0, Take: PageRequest.MaxTake + 1));
        }

        [Fact]
        public void Accepts_A_Page_Of_Exactly_The_Ceiling()
        {
            var request = new PageRequest(Skip: 0, Take: PageRequest.MaxTake);

            Assert.Equal(PageRequest.MaxTake, request.Take);
        }
    }
}
