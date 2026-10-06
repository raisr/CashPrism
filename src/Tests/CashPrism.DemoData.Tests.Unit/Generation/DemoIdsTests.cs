using CashPrism.DemoData.Generation;

namespace CashPrism.DemoData.Tests.Unit.Generation;

public sealed class DemoIdsTests
{
    public sealed class BookingId
    {
        [Fact]
        public void Is_40_Lower_Case_Hex_Characters()
            => Assert.Matches("^[0-9a-f]{40}$", DemoIds.BookingId(1));

        [Fact]
        public void Differs_Between_Two_Sequence_Numbers()
            => Assert.NotEqual(DemoIds.BookingId(1), DemoIds.BookingId(2));
    }

    public sealed class ContractId
    {
        [Fact]
        public void Is_32_Lower_Case_Hex_Characters()
            => Assert.Matches("^[0-9a-f]{32}$", DemoIds.ContractId("Strom"));
    }

    public sealed class Uuid
    {
        [Fact]
        public void Is_A_Lower_Case_Uuid_With_Dashes()
            => Assert.Matches("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", DemoIds.Uuid("Kreditkarte"));
    }

    public sealed class Digits
    {
        [Fact]
        public void Gives_As_Many_Digits_As_Asked_For()
            => Assert.Matches("^[0-9]{11}$", DemoIds.Digits("key", 11));

        [Fact]
        public void Gives_The_Same_Digits_For_The_Same_Key()
            => Assert.Equal(DemoIds.Digits("key", 40), DemoIds.Digits("key", 40));
    }
}
