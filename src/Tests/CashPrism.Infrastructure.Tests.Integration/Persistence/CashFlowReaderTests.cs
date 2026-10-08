using CashPrism.Application.CashFlow;
using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Bookings;
using CashPrism.TestSupport.Imports;
using CashPrism.TestSupport.Samples;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The sums behind the overview, against a real SQLite file. Which bookings
/// count is the domain rule's business and tested there; what only a database
/// shows is that the rule, the grouping by month and the sums translate into
/// SQL that adds up to the same cents.
/// </summary>
public sealed class CashFlowReaderTests
{
    private static readonly Guid ARun = Guid.Parse("4c1d2e3f-5a6b-4c7d-8e9f-0a1b2c3d4e5f");
    private static readonly DateOnly September = new(2026, 9, 1);
    private static readonly DateOnly October = new(2026, 10, 1);

    private static Booking CreateBooking(
        int ordinal,
        DateTime bookedOn,
        long amountInCents,
        string category = "Lebensmittel",
        bool isTransfer = false,
        SplitRole splitRole = SplitRole.None)
        => TestBookings.Create(
            ordinal.ToString("D40", null),
            bookedOn,
            amountInCents,
            category: category,
            isTransfer: isTransfer,
            splitRole: splitRole,
            originalFingerprint: splitRole is SplitRole.Part or SplitRole.Remainder ? TestBookings.AFingerprint : null,
            sourceImportRunId: ARun);

    private static ImportRun CreateRun(int ordinal, DateOnly? exportedOn, DateTimeOffset importedAt)
        => new(
            Guid.Parse($"00000000-0000-0000-0000-{ordinal:D12}"),
            fileName: "buchungen.xlsx",
            fileHash: $"hash{ordinal}",
            sheetName: "Tabelle1",
            exportedOn,
            importedAt);

    private static async Task SeedAsync(ThrowawayDatabase database, params Booking[] bookings)
    {
        await using var context = database.CreateContext();

        context.Bookings.AddRange(bookings);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAsync(ThrowawayDatabase database, params ImportRun[] runs)
    {
        await using var context = database.CreateContext();

        context.ImportRuns.AddRange(runs);

        await context.SaveChangesAsync();
    }

    public sealed class ReadCurrentToAsync
    {
        [Fact]
        public async Task Returns_Nothing_When_Nothing_Was_Imported()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await using var context = database.CreateContext();

            var currentTo = await new CashFlowReader(context).ReadCurrentToAsync();

            Assert.Null(currentTo);
        }

        [Fact]
        public async Task Returns_The_Latest_Export_Date_Whatever_Order_The_Files_Came_In()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateRun(1, new DateOnly(2026, 10, 1), new DateTimeOffset(2026, 10, 2, 8, 0, 0, TimeSpan.Zero)),
                CreateRun(2, new DateOnly(2026, 6, 30), new DateTimeOffset(2026, 10, 5, 8, 0, 0, TimeSpan.Zero)));
            await using var context = database.CreateContext();

            var currentTo = await new CashFlowReader(context).ReadCurrentToAsync();

            Assert.Equal(new DateOnly(2026, 10, 1), currentTo);
        }

        [Fact]
        public async Task Falls_Back_To_The_Day_Of_The_Latest_Import_When_No_Run_Carries_An_Export_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateRun(1, null, new DateTimeOffset(2026, 9, 3, 8, 0, 0, TimeSpan.Zero)),
                CreateRun(2, null, new DateTimeOffset(2026, 10, 2, 8, 0, 0, TimeSpan.Zero)));
            await using var context = database.CreateContext();

            var currentTo = await new CashFlowReader(context).ReadCurrentToAsync();

            Assert.Equal(new DateOnly(2026, 10, 2), currentTo);
        }
    }

    public sealed class ReadMonthsAsync
    {
        [Fact]
        public async Task Sums_Income_And_Spending_Per_Month_Oldest_First()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, new DateTime(2026, 9, 1), 400000),
                CreateBooking(2, new DateTime(2026, 9, 15), -6317),
                CreateBooking(3, new DateTime(2026, 9, 20), -1200),
                CreateBooking(4, new DateTime(2026, 10, 1), -4500));
            await using var context = database.CreateContext();

            var months = await new CashFlowReader(context).ReadMonthsAsync(September, October);

            Assert.Equal(
                [new MonthlyCashFlow(September, 400000, 7517), new MonthlyCashFlow(October, 0, 4500)],
                months);
        }

        [Fact]
        public async Task Applies_The_Counting_Rule_In_The_Query()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, new DateTime(2026, 9, 10), -10000, isTransfer: true),
                CreateBooking(2, new DateTime(2026, 9, 12), -15000, splitRole: SplitRole.Original),
                CreateBooking(3, new DateTime(2026, 9, 12), -6000, splitRole: SplitRole.Part),
                CreateBooking(4, new DateTime(2026, 9, 12), -9000, splitRole: SplitRole.Remainder));
            await using var context = database.CreateContext();

            var months = await new CashFlowReader(context).ReadMonthsAsync(September, September);

            Assert.Equal([new MonthlyCashFlow(September, 0, 15000)], months);
        }

        [Fact]
        public async Task Keeps_A_Booking_Posted_During_The_Last_Day_Of_The_Range()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, new DateTime(2026, 9, 30, 21, 14, 0), -2500),
                CreateBooking(2, new DateTime(2026, 8, 31, 23, 59, 0), -9900),
                CreateBooking(3, new DateTime(2026, 10, 1, 0, 0, 0), -9900));
            await using var context = database.CreateContext();

            var months = await new CashFlowReader(context).ReadMonthsAsync(September, September);

            Assert.Equal([new MonthlyCashFlow(September, 0, 2500)], months);
        }

        [Fact]
        public async Task Leaves_Out_A_Month_Without_A_Counted_Booking()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, CreateBooking(1, new DateTime(2026, 10, 2), -4500));
            await using var context = database.CreateContext();

            var months = await new CashFlowReader(context).ReadMonthsAsync(September, October);

            Assert.Equal([new MonthlyCashFlow(October, 0, 4500)], months);
        }
    }

    public sealed class ReadSpendingByCategoryAsync
    {
        [Fact]
        public async Task Sums_Spending_Per_Main_Category_Largest_First()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, new DateTime(2026, 9, 3), -6317, category: "Lebensmittel"),
                CreateBooking(2, new DateTime(2026, 9, 4), -120000, category: "Wohnen"),
                CreateBooking(3, new DateTime(2026, 9, 5), -2000, category: "Lebensmittel"),
                CreateBooking(4, new DateTime(2026, 10, 1), -99999, category: "Lebensmittel"));
            await using var context = database.CreateContext();

            var categories = await new CashFlowReader(context).ReadSpendingByCategoryAsync(September);

            Assert.Equal(
                [new CategorySpending("Wohnen", 120000), new CategorySpending("Lebensmittel", 8317)],
                categories);
        }

        [Fact]
        public async Task Leaves_Out_A_Category_That_Only_Brought_Money_In()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(
                database,
                CreateBooking(1, new DateTime(2026, 9, 1), 400000, category: "Einkommen"),
                CreateBooking(2, new DateTime(2026, 9, 3), -6317, category: "Lebensmittel"));
            await using var context = database.CreateContext();

            var categories = await new CashFlowReader(context).ReadSpendingByCategoryAsync(September);

            Assert.Equal([new CategorySpending("Lebensmittel", 6317)], categories);
        }
    }

    /// <summary>
    /// The committed demo export, imported the way the application imports it.
    /// May 2024 is the month of its one split booking and holds transfers
    /// between the household's own accounts besides. The expected cents were
    /// measured from the workbook itself, outside CashPrism.
    /// </summary>
    public sealed class WithTheDemoExport
    {
        private static readonly DateOnly May2024 = new(2024, 5, 1);

        private static async Task<ThrowawayDatabase> ImportDemoSampleAsync()
        {
            var database = await ThrowawayDatabase.CreateAsync();

            await using var context = database.CreateContext();
            await using var stream = DemoSample.Open();

            var importer = new Importer(
                new FinanzguruImportSource(),
                new ImportStore(context),
                new FixedClock(new DateTimeOffset(2026, 10, 2, 8, 0, 0, TimeSpan.Zero)));

            await importer.ImportAsync(stream, DemoSample.FileName);

            return database;
        }

        [Fact]
        public async Task Sums_The_Month_Of_The_Split_Booking()
        {
            await using var database = await ImportDemoSampleAsync();
            await using var context = database.CreateContext();

            var months = await new CashFlowReader(context).ReadMonthsAsync(May2024, May2024);

            Assert.Equal([new MonthlyCashFlow(May2024, 376200, 238240)], months);
        }

        [Fact]
        public async Task Counts_The_Split_Booking_Once_In_Its_Category()
        {
            await using var database = await ImportDemoSampleAsync();
            await using var context = database.CreateContext();

            var categories = await new CashFlowReader(context).ReadSpendingByCategoryAsync(May2024);

            Assert.Contains(new CategorySpending("Lifestyle", 29086), categories);
        }

        [Fact]
        public async Task Is_Current_To_The_Export_Date()
        {
            await using var database = await ImportDemoSampleAsync();
            await using var context = database.CreateContext();

            var currentTo = await new CashFlowReader(context).ReadCurrentToAsync();

            Assert.Equal(new DateOnly(2026, 10, 1), currentTo);
        }
    }
}
