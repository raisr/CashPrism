using CashPrism.Application.Imports;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Samples;
using CashPrism.TestSupport.Xlsx;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CashPrism.Shell.Tests.Integration.Imports;

/// <summary>
/// The import through the objects the host actually wires: the real export
/// reader, the real use case and the real database.
/// </summary>
/// <remarks>
/// Every rule below is asserted elsewhere against a test double, and those tests
/// are the ones that say why a rule exists. This one exists for what a double
/// cannot show: that a booking built by the reader is one the store can write,
/// that a second run reads back the state the first one wrote, and that the
/// container hands out an import that works at all — a registration nobody else
/// covers.
/// </remarks>
public sealed class ImportEndToEndTests
{
    private const string AFingerprint = FinanzguruTestRow.AFingerprint;
    private const string AnotherFingerprint = FinanzguruTestRow.AnotherFingerprint;

    /// <summary>
    /// A workbook of two bookings, as an export taken on <paramref name="exportedOn"/>
    /// would carry them. The same arguments always produce the same bytes, which
    /// is what lets a test import one file twice.
    /// </summary>
    private static byte[] Workbook(string exportedOn, string counterparty = "Supermarkt")
        => XlsxTestWorkbook.Build(
            FinanzguruColumns.All,
            [
                FinanzguruTestRow.Create(AFingerprint, counterparty: counterparty),
                FinanzguruTestRow.Create(AnotherFingerprint, amount: "-12.00"),
            ],
            sheetName: $"{exportedOn}_Export_Alle_Buchungen");

    private static async Task<ImportResult> ImportAsync(
        CashPrismWebApplicationFactory factory,
        byte[] workbook,
        string fileName = "20260907_Export_Alle_Buchungen.xlsx")
    {
        await using var scope = factory.Services.CreateAsyncScope();
        using var stream = new MemoryStream(workbook);

        return await scope.ServiceProvider
            .GetRequiredService<Importer>()
            .ImportAsync(stream, fileName);
    }

    /// <summary>
    /// A host on a database of its own, already migrated. Every test starts one:
    /// what is stored is what the test under way put there, and an import that
    /// found a booking a neighbouring test had inserted would count it as
    /// unchanged and quietly assert nothing.
    /// </summary>
    private static CashPrismWebApplicationFactory StartHost()
    {
        var factory = new CashPrismWebApplicationFactory();

        // Building the host is what applies the schema — see HostBootTests.
        factory.CreateClient().Dispose();

        return factory;
    }

    private static async Task<T> ReadAsync<T>(
        CashPrismWebApplicationFactory factory,
        Func<CashPrismDbContext, Task<T>> read)
    {
        await using var scope = factory.Services.CreateAsyncScope();

        return await read(scope.ServiceProvider.GetRequiredService<CashPrismDbContext>());
    }

    public sealed class AnImport
    {
        [Fact]
        public async Task Stores_What_The_Export_Described()
        {
            using var factory = StartHost();

            var result = await ImportAsync(factory, Workbook("20260907"));

            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Equal(2, result.RowsRead);
            Assert.Equal(2, result.BookingsInserted);

            var bookings = await ReadAsync(
                factory,
                context => context.Bookings.OrderBy(booking => booking.Fingerprint).ToListAsync());

            Assert.Equal([AFingerprint, AnotherFingerprint], bookings.Select(b => b.Fingerprint));
            Assert.Equal(-6317L, bookings[0].AmountInCents);
            Assert.Equal("Supermarkt", bookings[0].Counterparty);
        }

        [Fact]
        public async Task Records_The_Run_It_Was_Made_By()
        {
            using var factory = StartHost();

            var result = await ImportAsync(factory, Workbook("20260901"), "was-auch-immer.xlsx");

            var run = await ReadAsync(
                factory,
                context => context.ImportRuns.SingleAsync(r => r.Id == result.ImportRunId));

            Assert.True(run.IsComplete);
            Assert.Equal("was-auch-immer.xlsx", run.FileName);
            Assert.Equal("20260901_Export_Alle_Buchungen", run.SheetName);
            Assert.Equal(new DateOnly(2026, 9, 1), run.ExportedOn);
            Assert.Equal(2, run.RowsRead);
        }

        [Fact]
        public async Task Keeps_The_Row_Behind_Every_Stored_Booking()
        {
            using var factory = StartHost();

            var result = await ImportAsync(factory, Workbook("20260902"));

            var rows = await ReadAsync(
                factory,
                context => context.RawRows.Where(r => r.ImportRunId == result.ImportRunId).ToListAsync());

            Assert.Equal(2, rows.Count);
            Assert.All(rows, row => Assert.Contains("\"Buchungs-ID\":", row.Json, StringComparison.Ordinal));
        }

        /// <summary>
        /// The demo export is what a person without an export of their own
        /// imports first, so it has to go through the import the host wires,
        /// not only through the reader.
        /// </summary>
        [Fact]
        public async Task Of_The_Demo_Sample_Stores_Every_Booking_It_Carries()
        {
            using var factory = StartHost();
            using var sample = new MemoryStream();
            await using (var stream = DemoSample.Open())
            {
                await stream.CopyToAsync(sample);
            }

            var result = await ImportAsync(factory, sample.ToArray(), DemoSample.FileName);

            Assert.Equal(ImportOutcome.Imported, result.Outcome);
            Assert.Equal(result.RowsRead, result.BookingsInserted);
            Assert.Equal(result.RowsRead, await ReadAsync(factory, context => context.Bookings.CountAsync()));
        }
    }

    public sealed class ASecondImport
    {
        [Fact]
        public async Task Of_The_Same_File_Changes_Nothing()
        {
            using var factory = StartHost();
            var workbook = Workbook("20260907");
            var first = await ImportAsync(factory, workbook);

            var second = await ImportAsync(factory, workbook);

            Assert.Equal(ImportOutcome.AlreadyImported, second.Outcome);
            Assert.Equal(first.ImportRunId, second.ImportRunId);
            Assert.Equal(0, second.BookingsInserted);
            Assert.Equal(0, second.BookingsUpdated);
            Assert.Equal(2, await ReadAsync(factory, context => context.Bookings.CountAsync()));
            Assert.Equal(1, await ReadAsync(factory, context => context.ImportRuns.CountAsync()));
        }

        [Fact]
        public async Task Of_A_Later_Export_Replaces_The_Booking_It_Describes_Differently()
        {
            using var factory = StartHost();
            await ImportAsync(factory, Workbook("20260906"));

            var second = await ImportAsync(factory, Workbook("20260907", counterparty: "Supermarkt GmbH"));

            Assert.Equal(ImportOutcome.Imported, second.Outcome);
            Assert.Equal(1, second.BookingsUpdated);
            Assert.Equal(1, second.BookingsUnchanged);
            Assert.Equal(0, second.BookingsInserted);

            var booking = await ReadAsync(
                factory,
                context => context.Bookings.SingleAsync(b => b.Fingerprint == AFingerprint));

            Assert.Equal("Supermarkt GmbH", booking.Counterparty);
            Assert.Equal(second.ImportRunId, booking.SourceImportRunId);
        }

        [Fact]
        public async Task Of_A_Later_Export_Leaves_Both_States_Of_That_Booking_As_Rows()
        {
            using var factory = StartHost();
            await ImportAsync(factory, Workbook("20260906"));

            await ImportAsync(factory, Workbook("20260907", counterparty: "Supermarkt GmbH"));

            var rows = await ReadAsync(
                factory,
                context => context.RawRows.Where(r => r.Fingerprint == AFingerprint).ToListAsync());

            Assert.Equal(2, rows.Count);
            Assert.Contains(rows, row => row.Json.Contains("\"Supermarkt\"", StringComparison.Ordinal));
            Assert.Contains(rows, row => row.Json.Contains("\"Supermarkt GmbH\"", StringComparison.Ordinal));

            // The booking that did not change contributed no second row.
            Assert.Equal(3, await ReadAsync(factory, context => context.RawRows.CountAsync()));
        }

        [Fact]
        public async Task Of_An_Earlier_Export_Does_Not_Undo_The_Newer_One()
        {
            using var factory = StartHost();
            await ImportAsync(factory, Workbook("20260907", counterparty: "Supermarkt GmbH"));

            var second = await ImportAsync(factory, Workbook("20260906"));

            Assert.Equal(2, second.BookingsUnchanged);
            Assert.Equal(0, second.BookingsUpdated);

            var booking = await ReadAsync(
                factory,
                context => context.Bookings.SingleAsync(b => b.Fingerprint == AFingerprint));

            Assert.Equal("Supermarkt GmbH", booking.Counterparty);
        }
    }
}
