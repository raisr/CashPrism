using CashPrism.Application.Bookings;
using CashPrism.Domain.Bookings;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Bookings;

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
        => TestBookings.Create(
            Fingerprint(ordinal),
            bookedOn,
            amountInCents,
            accountName: accountName,
            counterparty: counterparty,
            paymentReference: paymentReference,
            category: category,
            sourceImportRunId: ARun);

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

            return [.. page.Items.Select(booking => booking.Fingerprint)];
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

            Assert.Equal(25, page.Items.Count);
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

            Assert.Empty(page.Items);
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

            var booking = Assert.Single(page.Items);
            Assert.Equal(-6317, booking.AmountInCents);
            Assert.Equal("EUR", booking.Currency);
            Assert.Equal("Girokonto", booking.AccountName);
            Assert.Equal("Supermarkt", booking.Counterparty);
            Assert.Equal("Wocheneinkauf", booking.PaymentReference);
            Assert.Equal("Lebensmittel", booking.Category);
        }

        [Fact]
        public async Task Reads_A_Booking_Back_With_The_Contract_Direct_Debit_And_Analysis_Fields()
        {
            var stored = TestBookings.Create(
                Fingerprint(1),
                reportedBalanceInCents: -36960546,
                transactionKind: "SEPA-Lastschrift",
                isContract: true,
                contractInterval: "vierteljaehrlich",
                contractId: "0a1b2c3d4e5f60718293a4b5c6d7e8f9",
                isExcludedFromDisposableIncome: true,
                mandateReference: "KM-1234567890-000001",
                creditorId: "DE98ZZZ09999999999",
                tags: "Urlaub",
                sourceImportRunId: ARun);
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, stored);
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new BookingPageRequest(Skip: 0, Take: 25));

            Assert.Equivalent(stored, Assert.Single(page.Items), strict: true);
        }
    }

    /// <summary>
    /// Each criterion of the filter, against bookings that differ in exactly
    /// that criterion, so a test fails for the one it is about.
    /// </summary>
    public sealed class ReadPageAsyncWithAFilter
    {
        private static async Task<IReadOnlyList<string>> ListAsync(BookingFilter filter, params Booking[] bookings)
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, bookings);
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(new BookingPageRequest(Skip: 0, Take: 25, Filter: filter));

            return [.. page.Items.Select(booking => booking.Fingerprint).Order(StringComparer.Ordinal)];
        }

        [Fact]
        public async Task Lists_Only_The_Bookings_Of_The_Account()
        {
            var listed = await ListAsync(
                new BookingFilter(AccountReference: "DE02120300000000202051"),
                TestBookings.Create(Fingerprint(1), accountReference: "DE02120300000000202051", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(2), accountReference: "DE88100900001234567892", sourceImportRunId: ARun));

            Assert.Equal([Fingerprint(1)], listed);
        }

        [Fact]
        public async Task Lists_Only_The_Bookings_Of_The_Main_Category()
        {
            var listed = await ListAsync(
                new BookingFilter(Category: "Lebensmittel"),
                CreateBooking(1, category: "Lebensmittel"),
                CreateBooking(2, category: "Mobilitaet"));

            Assert.Equal([Fingerprint(1)], listed);
        }

        [Fact]
        public async Task Lists_Only_The_Bookings_Of_The_Sub_Category()
        {
            var listed = await ListAsync(
                new BookingFilter(Category: "Lebensmittel", SubCategory: "Supermarkt"),
                TestBookings.Create(Fingerprint(1), category: "Lebensmittel", subCategory: "Supermarkt", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(2), category: "Lebensmittel", subCategory: "Baeckerei", sourceImportRunId: ARun));

            Assert.Equal([Fingerprint(1)], listed);
        }

        [Fact]
        public async Task Lists_The_Bookings_Of_Both_Ends_Of_The_Period_And_None_Outside_It()
        {
            var listed = await ListAsync(
                new BookingFilter(Period: new BookingPeriod(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30))),
                CreateBooking(1, bookedOn: new DateTime(2026, 8, 31, 23, 59, 0, DateTimeKind.Unspecified)),
                CreateBooking(2, bookedOn: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Unspecified)),
                CreateBooking(3, bookedOn: new DateTime(2026, 9, 30, 18, 30, 0, DateTimeKind.Unspecified)),
                CreateBooking(4, bookedOn: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Unspecified)));

            Assert.Equal([Fingerprint(2), Fingerprint(3)], listed);
        }

        [Fact]
        public async Task Lists_Only_The_Bookings_Of_The_Counterparty_As_Written()
        {
            var listed = await ListAsync(
                new BookingFilter(Counterparty: "Supermarkt"),
                CreateBooking(1, counterparty: "Supermarkt"),
                CreateBooking(2, counterparty: "Supermarkt Nord"));

            Assert.Equal([Fingerprint(1)], listed);
        }

        [Theory]
        [InlineData(BookingDirection.Income, 1)]
        [InlineData(BookingDirection.Spending, 2)]
        public async Task Lists_Only_The_Bookings_Of_The_Direction(BookingDirection direction, int expected)
        {
            var listed = await ListAsync(
                new BookingFilter(Direction: direction),
                CreateBooking(1, amountInCents: 250000),
                CreateBooking(2, amountInCents: -6317));

            Assert.Equal([Fingerprint(expected)], listed);
        }

        [Fact]
        public async Task Lists_Transfers_Unless_They_Are_Hidden()
        {
            Booking[] bookings =
            [
                CreateBooking(1),
                TestBookings.Create(Fingerprint(2), isTransfer: true, sourceImportRunId: ARun),
            ];

            var shown = await ListAsync(new BookingFilter(), bookings);
            var hidden = await ListAsync(new BookingFilter(HideTransfers: true), bookings);

            Assert.Equal([Fingerprint(1), Fingerprint(2)], shown);
            Assert.Equal([Fingerprint(1)], hidden);
        }

        [Fact]
        public async Task Lists_Only_The_Bookings_That_Meet_Every_Criterion()
        {
            var listed = await ListAsync(
                new BookingFilter(Category: "Lebensmittel", Direction: BookingDirection.Spending),
                CreateBooking(1, amountInCents: -6317, category: "Lebensmittel"),
                CreateBooking(2, amountInCents: 1200, category: "Lebensmittel"),
                CreateBooking(3, amountInCents: -6317, category: "Mobilitaet"));

            Assert.Equal([Fingerprint(1)], listed);
        }

        [Fact]
        public async Task Counts_Only_The_Filtered_Bookings_In_The_Total()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, category: "Lebensmittel"),
                CreateBooking(2, category: "Mobilitaet"),
                CreateBooking(3, category: "Mobilitaet"));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var page = await reader.ReadPageAsync(
                new BookingPageRequest(Skip: 0, Take: 1, Filter: new BookingFilter(Category: "Mobilitaet")));

            Assert.Equal(2, page.TotalCount);
        }
    }

    public sealed class ReadSummaryAsync
    {
        private static async Task<BookingSummary> SumAsync(BookingFilter filter, params Booking[] bookings)
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, bookings);
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            return await reader.ReadSummaryAsync(filter);
        }

        [Fact]
        public async Task Adds_Up_Income_And_Spending_Apart()
        {
            var summary = await SumAsync(
                new BookingFilter(),
                CreateBooking(1, amountInCents: 250000),
                CreateBooking(2, amountInCents: -6317),
                CreateBooking(3, amountInCents: -1200));

            Assert.Equal(new BookingSummary(3, 250000, 7517), summary);
        }

        [Fact]
        public async Task Is_Zero_Over_A_Filter_That_Lists_Nothing()
        {
            var summary = await SumAsync(new BookingFilter(Category: "Mobilitaet"), CreateBooking(1));

            Assert.Equal(new BookingSummary(0, 0, 0), summary);
        }

        [Fact]
        public async Task Lists_The_Original_Of_A_Split_Booking_But_Sums_Only_Its_Parts()
        {
            var summary = await SumAsync(
                new BookingFilter(),
                TestBookings.Create(Fingerprint(1), amountInCents: -10000, splitRole: SplitRole.Original, sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(2), amountInCents: -6000, splitRole: SplitRole.Part, originalFingerprint: Fingerprint(1), sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(3), amountInCents: -4000, splitRole: SplitRole.Remainder, originalFingerprint: Fingerprint(1), sourceImportRunId: ARun));

            Assert.Equal(new BookingSummary(3, 0, 10000), summary);
        }

        [Fact]
        public async Task Sums_Transfers_While_They_Are_Shown()
        {
            var summary = await SumAsync(
                new BookingFilter(),
                CreateBooking(1, amountInCents: -6317),
                TestBookings.Create(Fingerprint(2), amountInCents: -50000, isTransfer: true, sourceImportRunId: ARun));

            Assert.Equal(new BookingSummary(2, 0, 56317), summary);
        }

        [Fact]
        public async Task Leaves_Transfers_Out_Of_The_Count_And_The_Sums_When_They_Are_Hidden()
        {
            var summary = await SumAsync(
                new BookingFilter(HideTransfers: true),
                CreateBooking(1, amountInCents: -6317),
                TestBookings.Create(Fingerprint(2), amountInCents: -50000, isTransfer: true, sourceImportRunId: ARun));

            Assert.Equal(new BookingSummary(1, 0, 6317), summary);
        }

        [Fact]
        public async Task Sums_One_Account_Over_A_Month_Of_The_Demo_Export()
        {
            // Measured from the workbook: the Girokonto in September 2026,
            // its six transfers left out.
            await using var database = await DemoSampleDatabase.CreateAsync();
            await using var context = database.CreateContext();

            var summary = await new BookingReader(context).ReadSummaryAsync(new BookingFilter(
                AccountReference: "DE98999500006166400707",
                Period: new BookingPeriod(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30)),
                HideTransfers: true));

            Assert.Equal(new BookingSummary(21, 395200, 131987), summary);
        }
    }

    public sealed class ReadFilterOptionsAsync
    {
        [Fact]
        public async Task Offers_Each_Account_Once_By_Name()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                TestBookings.Create(Fingerprint(1), accountReference: "DE02120300000000202051", accountName: "Girokonto", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(2), accountReference: "DE02120300000000202051", accountName: "Girokonto", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(3), accountReference: "Paynet", accountName: "Paynet", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(4), accountReference: "DE88100900001234567892", accountName: "Altes Sparkonto", sourceImportRunId: ARun));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var options = await reader.ReadFilterOptionsAsync();

            Assert.Equal(
                [
                    new BookingAccount("DE88100900001234567892", "Altes Sparkonto"),
                    new BookingAccount("DE02120300000000202051", "Girokonto"),
                    new BookingAccount("Paynet", "Paynet"),
                ],
                options.Accounts);
        }

        [Fact]
        public async Task Offers_Each_Main_Category_With_Its_Own_Sub_Categories()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                TestBookings.Create(Fingerprint(1), category: "Mobilitaet", subCategory: "Tanken", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(2), category: "Lebensmittel", subCategory: "Supermarkt", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(3), category: "Lebensmittel", subCategory: "Baeckerei", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(4), category: "Lebensmittel", subCategory: "Supermarkt", sourceImportRunId: ARun));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var options = await reader.ReadFilterOptionsAsync();

            Assert.Equivalent(
                new[]
                {
                    new BookingCategory("Lebensmittel", ["Baeckerei", "Supermarkt"]),
                    new BookingCategory("Mobilitaet", ["Tanken"]),
                },
                options.Categories,
                strict: true);
        }

        [Fact]
        public async Task Offers_No_Empty_Category_Or_Sub_Category()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                TestBookings.Create(Fingerprint(1), category: "", subCategory: "", sourceImportRunId: ARun),
                TestBookings.Create(Fingerprint(2), category: "Sonstiges", subCategory: "", sourceImportRunId: ARun));
            var reader = CreateReader(database, out var context);
            await using var _ = context;

            var options = await reader.ReadFilterOptionsAsync();

            var category = Assert.Single(options.Categories);
            Assert.Equal(("Sonstiges", 0), (category.Name, category.SubCategories.Count));
        }
    }
}
