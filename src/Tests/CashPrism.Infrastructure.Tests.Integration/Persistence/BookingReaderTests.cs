using CashPrism.Application.Bookings;
using CashPrism.Domain.Bookings;
using CashPrism.Infrastructure.Persistence;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The read side of the booking list, against a real SQLite file. What a double
/// cannot show is what SQLite actually does with an <c>ORDER BY</c> over a set
/// of rows and a page taken out of the middle of it.
/// </summary>
public sealed class BookingReaderTests
{
    private static readonly Guid ARun = Guid.Parse("8f1c2d3e-4a5b-6c7d-8e9f-0a1b2c3d4e5f");

    /// <summary>
    /// A booking with everything but its fingerprint defaulted, so a test names
    /// only the field it is about.
    /// </summary>
    private static Booking CreateBooking(
        int ordinal,
        DateTime? bookedOn = null,
        long amountInCents = -6317,
        string accountName = "Girokonto",
        string counterparty = "Supermarkt",
        string paymentReference = "",
        string category = "Lebensmittel")
        => new(
            Fingerprint(ordinal),
            bookedOn ?? new DateTime(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified),
            amountInCents,
            currency: "EUR",
            accountReference: "DE02120300000000202051",
            accountName,
            counterparty,
            counterpartyAccount: string.Empty,
            paymentReference,
            category,
            subCategory: "Supermarkt",
            isTransfer: false,
            SplitRole.None,
            originalFingerprint: null,
            ARun);

    /// <summary>A fingerprint that sorts in the same order as its ordinal.</summary>
    private static string Fingerprint(int ordinal) => ordinal.ToString("D40", null);

    private static async Task SeedAsync(ThrowawayDatabase database, params Booking[] bookings)
    {
        await using var context = database.CreateContext();

        context.Bookings.AddRange(bookings);

        await context.SaveChangesAsync();
    }

    private static BookingReader CreateReader(ThrowawayDatabase database, out CashPrismDbContext context)
    {
        context = database.CreateContext();

        return new BookingReader(context);
    }

    public sealed class CountAsync
    {
        [Fact]
        public async Task Counts_Nothing_In_An_Empty_Database()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            Assert.Equal(0, await reader.CountAsync());
        }

        [Fact]
        public async Task Counts_Every_Stored_Booking()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, CreateBooking(1), CreateBooking(2), CreateBooking(3));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            Assert.Equal(3, await reader.CountAsync());
        }
    }

    public sealed class ReadPageAsync
    {
        private static async Task<IReadOnlyList<string>> FingerprintsAsync(
            ThrowawayDatabase database,
            BookingPageRequest request)
        {
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(request);

            return [.. page.Bookings.Select(booking => booking.Fingerprint)];
        }

        [Fact]
        public async Task Puts_The_Newest_Booking_First()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, bookedOn: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Unspecified)),
                CreateBooking(2, bookedOn: new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Unspecified)),
                CreateBooking(3, bookedOn: new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Unspecified)));

            var fingerprints = await FingerprintsAsync(database, new BookingPageRequest(Skip: 0, Take: 25));

            Assert.Equal([Fingerprint(2), Fingerprint(3), Fingerprint(1)], fingerprints);
        }

        [Fact]
        public async Task Reads_The_Oldest_Booking_First_When_The_Order_Is_Turned_Around()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, bookedOn: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Unspecified)),
                CreateBooking(2, bookedOn: new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Unspecified)));

            var fingerprints = await FingerprintsAsync(
                database,
                new BookingPageRequest(Skip: 0, Take: 25, Descending: false));

            Assert.Equal([Fingerprint(1), Fingerprint(2)], fingerprints);
        }

        [Fact]
        public async Task Orders_Amounts_By_Their_Value_Not_By_Their_Text()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, amountInCents: -900),
                CreateBooking(2, amountInCents: -1000),
                CreateBooking(3, amountInCents: 250_000));

            var fingerprints = await FingerprintsAsync(
                database,
                new BookingPageRequest(
                    Skip: 0,
                    Take: 25,
                    SortBy: BookingSortColumn.AmountInCents,
                    Descending: false));

            Assert.Equal([Fingerprint(2), Fingerprint(1), Fingerprint(3)], fingerprints);
        }

        [Theory]
        [InlineData(BookingSortColumn.AccountName)]
        [InlineData(BookingSortColumn.Counterparty)]
        [InlineData(BookingSortColumn.PaymentReference)]
        [InlineData(BookingSortColumn.Category)]
        public async Task Orders_By_The_Text_Column_It_Was_Asked_For(BookingSortColumn column)
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(
                    1,
                    accountName: "Zweitkonto",
                    counterparty: "Zahnarzt",
                    paymentReference: "Zahnersatz",
                    category: "Zuhause"),
                CreateBooking(
                    2,
                    accountName: "Auszahlkonto",
                    counterparty: "Apotheke",
                    paymentReference: "Arznei",
                    category: "Abos"));

            var fingerprints = await FingerprintsAsync(
                database,
                new BookingPageRequest(Skip: 0, Take: 25, column, Descending: false));

            Assert.Equal([Fingerprint(2), Fingerprint(1)], fingerprints);
        }

        [Fact]
        public async Task Hands_Out_Only_The_Slice_It_Was_Asked_For()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, [.. Enumerable.Range(1, 10).Select(n => CreateBooking(n))]);

            var fingerprints = await FingerprintsAsync(
                database,
                new BookingPageRequest(Skip: 4, Take: 3, Descending: false));

            Assert.Equal([Fingerprint(5), Fingerprint(6), Fingerprint(7)], fingerprints);
        }

        /// <summary>
        /// Every booking here shares one date, which is the ordinary case in an
        /// export: without the fingerprint as a second sort key SQLite is free to
        /// return them in a different order for every query, and a reader paging
        /// through the list would see one booking twice and never see another.
        /// </summary>
        [Fact]
        public async Task Pages_Do_Not_Overlap_When_Every_Booking_Shares_A_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, [.. Enumerable.Range(1, 60).Select(n => CreateBooking(n))]);

            var first = await FingerprintsAsync(database, new BookingPageRequest(Skip: 0, Take: 25));
            var second = await FingerprintsAsync(database, new BookingPageRequest(Skip: 25, Take: 25));
            var third = await FingerprintsAsync(database, new BookingPageRequest(Skip: 50, Take: 25));

            Assert.Equal(60, first.Concat(second).Concat(third).Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public async Task Reports_How_Many_Bookings_There_Are_Rather_Than_How_Many_It_Returned()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, [.. Enumerable.Range(1, 60).Select(n => CreateBooking(n))]);
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new BookingPageRequest(Skip: 0, Take: 25));

            Assert.Equal(25, page.Bookings.Count);
            Assert.Equal(60, page.TotalCount);
        }

        [Fact]
        public async Task Hands_Back_An_Empty_Page_Beyond_The_Last_Booking()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, CreateBooking(1));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new BookingPageRequest(Skip: 100, Take: 25));

            Assert.Empty(page.Bookings);
            Assert.Equal(1, page.TotalCount);
        }

        [Fact]
        public async Task Reads_A_Booking_Back_With_The_Fields_The_List_Shows()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, amountInCents: -6317, counterparty: "Supermarkt", paymentReference: "Wocheneinkauf"));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new BookingPageRequest(Skip: 0, Take: 25));

            var booking = Assert.Single(page.Bookings);
            Assert.Equal(-6317, booking.AmountInCents);
            Assert.Equal("EUR", booking.Currency);
            Assert.Equal("Girokonto", booking.AccountName);
            Assert.Equal("Supermarkt", booking.Counterparty);
            Assert.Equal("Wocheneinkauf", booking.PaymentReference);
            Assert.Equal("Lebensmittel", booking.Category);
        }
    }
}
