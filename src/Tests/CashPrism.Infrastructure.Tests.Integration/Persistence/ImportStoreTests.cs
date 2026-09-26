using CashPrism.Application.Imports;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;
using CashPrism.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class ImportStoreTests
{
    private const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";
    private const string AnotherFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    private static readonly DateTimeOffset AnImportTime = new(2026, 9, 7, 18, 0, 0, TimeSpan.Zero);

    private static ImportRun CreateRun(
        Guid id,
        string fileHash = "3b8f1c",
        DateOnly? exportedOn = null,
        DateTimeOffset? importedAt = null)
        => new(
            id,
            fileName: "20260907_Export_Alle_Buchungen.xlsx",
            fileHash,
            sheetName: "20260907_Export_Alle_Buchungen",
            exportedOn ?? new DateOnly(2026, 9, 7),
            importedAt ?? AnImportTime);

    private static Booking CreateBooking(Guid runId, string fingerprint = AFingerprint, long amountInCents = -6317)
        => new(
            fingerprint,
            new DateTime(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified),
            amountInCents,
            currency: "EUR",
            accountReference: "DE02120300000000202051",
            accountName: "Girokonto",
            counterparty: "Supermarkt",
            counterpartyAccount: string.Empty,
            paymentReference: string.Empty,
            category: "Lebensmittel",
            subCategory: "Supermarkt",
            isTransfer: false,
            SplitRole.None,
            originalFingerprint: null,
            runId);

    /// <summary>
    /// Writes one booking with the row behind it, the way an import would, and
    /// hands back the run it was written by.
    /// </summary>
    private static async Task<Guid> SeedAsync(
        ThrowawayDatabase database,
        string fingerprint = AFingerprint,
        string json = """{"Betrag":"-63.17"}""",
        string fileHash = "3b8f1c",
        DateOnly? exportedOn = null)
    {
        var runId = Guid.NewGuid();

        await using var context = database.CreateContext();
        var store = new ImportStore(context);

        await store.AddRunAsync(CreateRun(runId, fileHash, exportedOn));
        await store.ApplyAsync(new ImportBatch(
            [CreateBooking(runId, fingerprint)],
            BookingsToUpdate: [],
            [new RawRow(runId, fingerprint, json)]));

        return runId;
    }

    public sealed class FindRunByFileHashAsync
    {
        [Fact]
        public async Task Finds_The_Run_That_Imported_The_Same_File()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var runId = await SeedAsync(database, fileHash: "abc123");

            await using var context = database.CreateContext();
            var found = await new ImportStore(context).FindRunByFileHashAsync("abc123");

            Assert.Equal(runId, found!.Id);
        }

        [Fact]
        public async Task Finds_Nothing_For_A_File_That_Was_Never_Imported()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, fileHash: "abc123");

            await using var context = database.CreateContext();

            Assert.Null(await new ImportStore(context).FindRunByFileHashAsync("something else"));
        }
    }

    public sealed class LoadStatesAsync
    {
        [Fact]
        public async Task Returns_The_Row_The_Stored_Booking_Came_From()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var runId = await SeedAsync(database, json: """{"Betrag":"-63.17"}""");

            await using var context = database.CreateContext();
            var states = await new ImportStore(context).LoadStatesAsync([AFingerprint]);

            Assert.Equal("""{"Betrag":"-63.17"}""", states[AFingerprint].RawJson);
            Assert.Equal(runId, states[AFingerprint].SourceRun.Id);
        }

        /// <summary>
        /// The run comes back whole, because the import compares export dates on
        /// it to decide which of two states is the later one.
        /// </summary>
        [Fact]
        public async Task Returns_The_Run_With_Its_Export_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, exportedOn: new DateOnly(2026, 9, 6));

            await using var context = database.CreateContext();
            var states = await new ImportStore(context).LoadStatesAsync([AFingerprint]);

            Assert.Equal(new DateOnly(2026, 9, 6), states[AFingerprint].SourceRun.ExportedOn);
        }

        [Fact]
        public async Task Leaves_Out_A_Fingerprint_That_Is_Not_Stored()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database);

            await using var context = database.CreateContext();
            var states = await new ImportStore(context).LoadStatesAsync([AFingerprint, AnotherFingerprint]);

            Assert.True(states.ContainsKey(AFingerprint));
            Assert.False(states.ContainsKey(AnotherFingerprint));
        }

        [Fact]
        public async Task Returns_Nothing_For_An_Empty_Batch()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using var context = database.CreateContext();

            Assert.Empty(await new ImportStore(context).LoadStatesAsync([]));
        }

        /// <summary>
        /// A stored booking whose newer state came from a later run has to report
        /// that later run, not the one that first wrote it.
        /// </summary>
        [Fact]
        public async Task Reports_The_Run_That_Last_Replaced_The_Booking()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, json: """{"Betrag":"-63.17"}""");
            var laterRunId = Guid.NewGuid();

            await using (var writing = database.CreateContext())
            {
                var store = new ImportStore(writing);
                await store.AddRunAsync(CreateRun(laterRunId, fileHash: "later", exportedOn: new DateOnly(2026, 9, 8)));
                await store.ApplyAsync(new ImportBatch(
                    BookingsToInsert: [],
                    [CreateBooking(laterRunId, AFingerprint, amountInCents: -100)],
                    [new RawRow(laterRunId, AFingerprint, """{"Betrag":"-1.00"}""")]));
            }

            await using var context = database.CreateContext();
            var state = (await new ImportStore(context).LoadStatesAsync([AFingerprint]))[AFingerprint];

            Assert.Equal(laterRunId, state.SourceRun.Id);
            Assert.Equal("""{"Betrag":"-1.00"}""", state.RawJson);
        }
    }

    public sealed class ApplyAsync
    {
        [Fact]
        public async Task Stores_A_New_Booking_With_Its_Row()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database);

            await using var context = database.CreateContext();

            Assert.Single(await context.Bookings.ToListAsync());
            Assert.Single(await context.RawRows.ToListAsync());
        }

        [Fact]
        public async Task Replaces_A_Stored_Booking_Rather_Than_Adding_A_Second()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database);
            var laterRunId = Guid.NewGuid();

            await using (var writing = database.CreateContext())
            {
                var store = new ImportStore(writing);
                await store.AddRunAsync(CreateRun(laterRunId, fileHash: "later"));
                await store.ApplyAsync(new ImportBatch(
                    BookingsToInsert: [],
                    [CreateBooking(laterRunId, AFingerprint, amountInCents: -100)],
                    [new RawRow(laterRunId, AFingerprint, """{"Betrag":"-1.00"}""")]));
            }

            await using var context = database.CreateContext();
            var booking = await context.Bookings.SingleAsync();

            Assert.Equal(-100L, booking.AmountInCents);
            Assert.Equal(laterRunId, booking.SourceImportRunId);
        }

        [Fact]
        public async Task Keeps_Both_States_Of_A_Replaced_Booking_As_Rows()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            await SeedAsync(database, json: """{"Betrag":"-63.17"}""");
            var laterRunId = Guid.NewGuid();

            await using (var writing = database.CreateContext())
            {
                var store = new ImportStore(writing);
                await store.AddRunAsync(CreateRun(laterRunId, fileHash: "later"));
                await store.ApplyAsync(new ImportBatch(
                    BookingsToInsert: [],
                    [CreateBooking(laterRunId, AFingerprint, amountInCents: -100)],
                    [new RawRow(laterRunId, AFingerprint, """{"Betrag":"-1.00"}""")]));
            }

            await using var context = database.CreateContext();

            Assert.Equal(2, await context.RawRows.CountAsync());
        }

        /// <summary>
        /// The tracker is what would otherwise hold every entity of a whole
        /// export, so the store has to let go of each batch.
        /// </summary>
        [Fact]
        public async Task Leaves_Nothing_Tracked_Behind()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using var context = database.CreateContext();
            var runId = Guid.NewGuid();
            var store = new ImportStore(context);

            await store.AddRunAsync(CreateRun(runId));
            await store.ApplyAsync(new ImportBatch(
                [CreateBooking(runId)],
                BookingsToUpdate: [],
                [new RawRow(runId, AFingerprint, """{"a":1}""")]));

            Assert.Empty(context.ChangeTracker.Entries());
        }
    }

    public sealed class CompleteRunAsync
    {
        [Fact]
        public async Task Writes_The_Counts_Onto_The_Run_That_Was_Already_Stored()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var runId = await SeedAsync(database);

            await using (var writing = database.CreateContext())
            {
                var run = await writing.ImportRuns.AsNoTracking().SingleAsync(r => r.Id == runId);
                run.Complete(rowsRead: 3, bookingsInserted: 1, bookingsUpdated: 0, bookingsUnchanged: 2);

                await new ImportStore(writing).CompleteRunAsync(run);
            }

            await using var context = database.CreateContext();
            var stored = await context.ImportRuns.SingleAsync();

            Assert.True(stored.IsComplete);
            Assert.Equal(3, stored.RowsRead);
            Assert.Equal(1, stored.BookingsInserted);
            Assert.Equal(2, stored.BookingsUnchanged);
        }

        [Fact]
        public async Task Does_Not_Add_A_Second_Run()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var runId = await SeedAsync(database);

            await using (var writing = database.CreateContext())
            {
                var run = await writing.ImportRuns.AsNoTracking().SingleAsync(r => r.Id == runId);
                run.Complete(rowsRead: 1, bookingsInserted: 1, bookingsUpdated: 0, bookingsUnchanged: 0);

                await new ImportStore(writing).CompleteRunAsync(run);
            }

            await using var context = database.CreateContext();

            Assert.Single(await context.ImportRuns.ToListAsync());
        }
    }
}
