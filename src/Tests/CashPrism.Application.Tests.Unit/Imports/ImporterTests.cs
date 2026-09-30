using System.Text;
using CashPrism.Application.Imports;

namespace CashPrism.Application.Tests.Unit.Imports;

public sealed class ImporterTests
{
    private const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";
    private const string AnotherFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    private static readonly DateTimeOffset AnImportTime = new(2026, 9, 7, 18, 0, 0, TimeSpan.Zero);

    private static Task<ImportResult> RunImport(
        FakeImportStore store,
        FakeImportSource source,
        DateTimeOffset? importedAt = null,
        string content = "an export")
    {
        var importer = new Importer(source, store, new FixedClock(importedAt ?? AnImportTime));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        return importer.ImportAsync(stream, fileName: "20260907_Export_Alle_Buchungen.xlsx");
    }

    public sealed class ImportAsync
    {
        [Fact]
        public async Task Stores_A_Booking_The_Database_Has_Never_Seen()
        {
            var store = new FakeImportStore();

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 7), (AFingerprint, """{"Betrag":"-63.17"}""")));

            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Equal(1, result.BookingsInserted);
            Assert.Equal(1, result.RowsRead);
            Assert.Single(store.Bookings);
        }

        [Fact]
        public async Task Keeps_The_Row_Behind_A_New_Booking()
        {
            var store = new FakeImportStore();

            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 7), (AFingerprint, """{"Betrag":"-63.17"}""")));

            Assert.Equal("""{"Betrag":"-63.17"}""", store.RawRows.Single().Json);
        }

        [Fact]
        public async Task Reports_The_Same_File_As_Already_Imported_Without_Writing_Again()
        {
            var store = new FakeImportStore();
            var row = (AFingerprint, """{"Betrag":"-63.17"}""");
            await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), row));

            var result = await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), row));

            Assert.Equal(ImportOutcome.AlreadyImported, result.Outcome);
            Assert.Equal(0, result.BookingsInserted);
            Assert.Equal(0, result.BookingsUpdated);
            Assert.Single(store.Bookings);
            Assert.Single(store.Runs);
        }

        [Fact]
        public async Task Names_The_Earlier_Run_That_Already_Holds_The_File()
        {
            var store = new FakeImportStore();
            var row = (AFingerprint, """{"Betrag":"-63.17"}""");
            var first = await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), row));

            var second = await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), row));

            Assert.Equal(first.ImportRunId, second.ImportRunId);
        }

        [Fact]
        public async Task Counts_An_Unchanged_Booking_Without_Storing_Its_Row_Again()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), (AFingerprint, """{"Betrag":"-63.17"}""")),
                content: "the first export");

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 7), (AFingerprint, """{"Betrag":"-63.17"}""")),
                content: "the second export");

            Assert.Equal(1, result.BookingsUnchanged);
            Assert.Equal(0, result.BookingsUpdated);
            Assert.Single(store.RawRows);
        }

        [Fact]
        public async Task Replaces_A_Booking_A_Later_Export_Describes_Differently()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), (AFingerprint, """{"Beguenstigter":""}""")),
                content: "the first export");

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(
                    new DateOnly(2026, 9, 7),
                    (AFingerprint, """{"Beguenstigter":"Supermarkt"}""")),
                content: "the second export");

            Assert.Equal(1, result.BookingsUpdated);
            Assert.Equal(0, result.BookingsInserted);
            Assert.Single(store.Bookings);
        }

        [Fact]
        public async Task Keeps_Both_States_Of_A_Changed_Booking_As_Raw_Rows()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), (AFingerprint, """{"Beguenstigter":""}""")),
                content: "the first export");

            await RunImport(
                store,
                FakeImportSource.Carrying(
                    new DateOnly(2026, 9, 7),
                    (AFingerprint, """{"Beguenstigter":"Supermarkt"}""")),
                content: "the second export");

            Assert.Equal(
                ["""{"Beguenstigter":""}""", """{"Beguenstigter":"Supermarkt"}"""],
                store.RawRows.Select(row => row.Json));
        }

        [Fact]
        public async Task Points_An_Updated_Booking_At_The_Run_That_Replaced_It()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), (AFingerprint, """{"Beguenstigter":""}""")),
                content: "the first export");

            var second = await RunImport(
                store,
                FakeImportSource.Carrying(
                    new DateOnly(2026, 9, 7),
                    (AFingerprint, """{"Beguenstigter":"Supermarkt"}""")),
                content: "the second export");

            Assert.Equal(second.ImportRunId, store.Bookings[AFingerprint].SourceImportRunId);
        }

        [Fact]
        public async Task Does_Not_Let_An_Older_Export_Undo_A_Newer_One()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(
                    new DateOnly(2026, 9, 7),
                    (AFingerprint, """{"Beguenstigter":"Supermarkt"}""")),
                content: "the newer export");

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), (AFingerprint, """{"Beguenstigter":""}""")),
                content: "the older export");

            Assert.Equal(0, result.BookingsUpdated);
            Assert.Equal(1, result.BookingsUnchanged);
            Assert.Equal(
                """{"Beguenstigter":"Supermarkt"}""",
                store.RawRows.Single().Json);
        }

        [Fact]
        public async Task Falls_Back_To_The_Import_Time_When_An_Export_Carries_No_Date()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(exportedOn: null, (AFingerprint, """{"Beguenstigter":""}""")),
                importedAt: AnImportTime,
                content: "the first export");

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(
                    exportedOn: null,
                    (AFingerprint, """{"Beguenstigter":"Supermarkt"}""")),
                importedAt: AnImportTime.AddHours(1),
                content: "the second export");

            Assert.Equal(1, result.BookingsUpdated);
        }

        [Fact]
        public async Task Records_What_The_Run_Did()
        {
            var store = new FakeImportStore();
            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), (AFingerprint, """{"a":1}""")),
                content: "the first export");

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(
                    new DateOnly(2026, 9, 7),
                    (AFingerprint, """{"a":2}"""),
                    (AnotherFingerprint, """{"b":1}""")),
                content: "the second export");

            var run = store.Runs[result.ImportRunId];

            Assert.True(store.WasCompleted);
            Assert.True(run.IsComplete);
            Assert.Equal(2, run.RowsRead);
            Assert.Equal(1, run.BookingsInserted);
            Assert.Equal(1, run.BookingsUpdated);
            Assert.Equal(0, run.BookingsUnchanged);
        }

        [Fact]
        public async Task Records_The_File_It_Was_Given()
        {
            var store = new FakeImportStore();

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 7), (AFingerprint, """{"a":1}""")));

            var run = store.Runs[result.ImportRunId];

            Assert.Equal("20260907_Export_Alle_Buchungen.xlsx", run.FileName);
            Assert.Equal("20260907_Export_Alle_Buchungen", run.SheetName);
            Assert.Equal(new DateOnly(2026, 9, 7), run.ExportedOn);
            Assert.Equal(AnImportTime, run.ImportedAt);
            Assert.NotEmpty(run.FileHash);
        }

        [Fact]
        public async Task Reads_The_File_For_The_Run_It_Created()
        {
            var store = new FakeImportStore();
            var source = FakeImportSource.Carrying(new DateOnly(2026, 9, 7), (AFingerprint, """{"a":1}"""));

            var result = await RunImport(store, source);

            Assert.Equal(result.ImportRunId, source.ReadForRunId);
        }

        [Fact]
        public async Task Passes_On_The_Columns_The_Export_Gained()
        {
            var store = new FakeImportStore();

            var result = await RunImport(
                store,
                FakeImportSource.CarryingUnknownColumns(
                    new DateOnly(2026, 9, 7),
                    ["Analyse-Nebelkerze"],
                    (AFingerprint, """{"a":1}""")));

            Assert.Equal(["Analyse-Nebelkerze"], result.UnknownColumns);
        }

        [Fact]
        public async Task Writes_Nothing_When_The_File_Cannot_Be_Read()
        {
            var store = new FakeImportStore();

            var result = await RunImport(store, FakeImportSource.Failing(ImportError.NoWorksheet()));

            Assert.Equal(ImportOutcome.Failed, result.Outcome);
            Assert.Equal([ImportError.NoWorksheet().Code], result.Errors.Select(error => error.Code));
            Assert.Empty(store.Runs);
            Assert.Empty(store.Bookings);
        }

        /// <summary>
        /// The booking id was distinct in all 6,324 measured rows, so a repeat means
        /// the assumption identity rests on has broken. Reported rather than left to
        /// fail as a duplicate key halfway through the write.
        /// </summary>
        [Fact]
        public async Task Refuses_A_File_That_Carries_One_Booking_Id_Twice()
        {
            var store = new FakeImportStore();

            var result = await RunImport(
                store,
                FakeImportSource.Carrying(
                    new DateOnly(2026, 9, 7),
                    (AFingerprint, """{"a":1}"""),
                    (AFingerprint, """{"a":2}""")));

            Assert.Equal(ImportOutcome.Failed, result.Outcome);
            Assert.Equivalent(ImportError.RepeatedBookingId(row: 3, firstRow: 2), result.Errors.Single(), strict: true);
            Assert.Empty(store.Runs);
        }

        [Fact]
        public async Task Refuses_A_Stream_It_Cannot_Rewind_To_Hash()
        {
            var importer = new Importer(
                FakeImportSource.Carrying(new DateOnly(2026, 9, 7)),
                new FakeImportStore(),
                new FixedClock(AnImportTime));
            using var stream = new UnseekableStream();

            await Assert.ThrowsAsync<ArgumentException>(
                () => importer.ImportAsync(stream, fileName: "export.xlsx"));
        }

        [Fact]
        public async Task Refuses_A_File_Without_A_Name()
        {
            var importer = new Importer(
                FakeImportSource.Carrying(new DateOnly(2026, 9, 7)),
                new FakeImportStore(),
                new FixedClock(AnImportTime));
            using var stream = new MemoryStream();

            await Assert.ThrowsAsync<ArgumentException>(() => importer.ImportAsync(stream, fileName: " "));
        }
    }

    /// <summary>
    /// An export carries the owner's whole history, so these say the work is cut
    /// into batches rather than held whole — see <see cref="Importer.BatchSize"/>.
    /// </summary>
    public sealed class Batching
    {
        [Fact]
        public async Task Looks_Up_The_Stored_State_A_Batch_At_A_Time()
        {
            var store = new FakeImportStore();
            var rows = Rows(Importer.BatchSize + 1);

            await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), rows));

            Assert.Equal([Importer.BatchSize, 1], store.LoadedBatchSizes);
        }

        [Fact]
        public async Task Writes_A_Batch_At_A_Time()
        {
            var store = new FakeImportStore();
            var rows = Rows(Importer.BatchSize + 1);

            await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), rows));

            Assert.Equal([Importer.BatchSize, 1], store.AppliedBatchSizes);
        }

        [Fact]
        public async Task Writes_Nothing_For_A_Batch_In_Which_Nothing_Changed()
        {
            var store = new FakeImportStore();
            var rows = Rows(3);
            await RunImport(
                store,
                FakeImportSource.Carrying(new DateOnly(2026, 9, 6), rows),
                content: "the first export");

            await RunImport(store, FakeImportSource.Carrying(new DateOnly(2026, 9, 7), rows), content: "again");

            // One write for the first import, none for the second: every row of it
            // said what was already stored.
            Assert.Equal([3], store.AppliedBatchSizes);
        }

        private static (string Fingerprint, string Payload)[] Rows(int count)
            => Enumerable
                .Range(0, count)
                .Select(index => ($"{index:x40}", $$"""{"row":{{index}}}"""))
                .ToArray();
    }
}
