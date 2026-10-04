using CashPrism.Web.Layout;

namespace CashPrism.Web.Tests.Unit.Layout;

public sealed class LibrarySummaryReaderTests
{
    public sealed class ReadAsync
    {
        private readonly FakeBookingReader bookings = new();
        private readonly FakeImportRunReader importRuns = new();

        [Fact]
        public async Task Counts_The_Stored_Bookings()
        {
            bookings.Count = 6327;

            var summary = await new LibrarySummaryReader(bookings, importRuns).ReadAsync();

            Assert.Equal(6327, summary.BookingCount);
        }

        [Fact]
        public async Task Dates_The_Library_By_The_Latest_Import_Run()
        {
            var latest = new DateTimeOffset(2026, 9, 7, 12, 0, 0, TimeSpan.Zero);
            importRuns.Add(latest.AddDays(-7));
            importRuns.Add(latest);

            var summary = await new LibrarySummaryReader(bookings, importRuns).ReadAsync();

            Assert.Equal(latest, summary.LastImportedAt);
        }

        [Fact]
        public async Task Has_No_Date_When_Nothing_Was_Ever_Imported()
        {
            var summary = await new LibrarySummaryReader(bookings, importRuns).ReadAsync();

            Assert.Null(summary.LastImportedAt);
        }
    }
}
