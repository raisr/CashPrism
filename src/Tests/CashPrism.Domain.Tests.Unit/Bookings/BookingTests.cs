using CashPrism.Domain.Bookings;

namespace CashPrism.Domain.Tests.Unit.Bookings;

public sealed class BookingTests
{
    private const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";
    private const string AnotherFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    private static Booking Create(
        string fingerprint = AFingerprint,
        DateTime? bookedOn = null,
        decimal amount = -12.34m,
        string currency = "EUR",
        string counterparty = "A shop",
        SplitRole splitRole = SplitRole.None,
        string? originalFingerprint = null)
        => new(
            fingerprint,
            bookedOn ?? new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Unspecified),
            amount,
            currency,
            accountReference: "DE02120300000000202051",
            accountName: "Current account",
            counterparty,
            counterpartyAccount: string.Empty,
            paymentReference: string.Empty,
            category: "Groceries",
            subCategory: "Supermarket",
            isTransfer: false,
            splitRole,
            originalFingerprint);

    public sealed class Constructor
    {
        [Fact]
        public void Refuses_A_Blank_Fingerprint()
            => Assert.Throws<ArgumentException>(() => Create(fingerprint: "  "));

        [Fact]
        public void Refuses_A_Blank_Currency()
            => Assert.Throws<ArgumentException>(() => Create(currency: ""));

        [Fact]
        public void Refuses_An_Unknown_Split_Role()
            => Assert.Throws<ArgumentOutOfRangeException>(() => Create(splitRole: (SplitRole)42));

        [Fact]
        public void Refuses_A_Split_Part_That_Does_Not_Say_Which_Booking_It_Belongs_To()
            => Assert.Throws<ArgumentException>(
                () => Create(splitRole: SplitRole.Part, originalFingerprint: null));

        [Fact]
        public void Accepts_A_Split_Original_Without_A_Back_Reference()
        {
            var booking = Create(splitRole: SplitRole.Original, originalFingerprint: null);

            Assert.Equal(SplitRole.Original, booking.SplitRole);
        }

        [Fact]
        public void Keeps_The_Time_Component_The_Export_Carries_On_Some_Rows()
        {
            var bookedOn = new DateTime(2026, 3, 14, 9, 41, 0, DateTimeKind.Unspecified);

            var booking = Create(bookedOn: bookedOn);

            Assert.Equal(bookedOn, booking.BookedOn);
        }
    }

    public sealed class IsSameBookingAs
    {
        [Fact]
        public void Says_Yes_When_The_Fingerprints_Match_Although_The_Other_Fields_Differ()
        {
            var stored = Create(counterparty: "UNKNOWN", amount: -12.34m);
            var reimported = Create(counterparty: "A shop, enriched later", amount: -12.34m);

            Assert.True(stored.IsSameBookingAs(reimported));
        }

        [Fact]
        public void Says_No_When_The_Fingerprints_Differ_Although_Every_Other_Field_Matches()
        {
            var one = Create(fingerprint: AFingerprint);
            var another = Create(fingerprint: AnotherFingerprint);

            Assert.False(one.IsSameBookingAs(another));
        }
    }

    public sealed class IsSplitPart
    {
        [Theory]
        [InlineData(SplitRole.Part)]
        [InlineData(SplitRole.Remainder)]
        public void Says_Yes_For_A_Role_Whose_Amount_Is_Already_Counted_By_Its_Original(SplitRole splitRole)
        {
            var booking = Create(splitRole: splitRole, originalFingerprint: AnotherFingerprint);

            Assert.True(booking.IsSplitPart);
        }

        [Theory]
        [InlineData(SplitRole.None)]
        [InlineData(SplitRole.Original)]
        public void Says_No_For_A_Role_That_Carries_Its_Own_Amount(SplitRole splitRole)
        {
            var booking = Create(splitRole: splitRole);

            Assert.False(booking.IsSplitPart);
        }
    }
}
