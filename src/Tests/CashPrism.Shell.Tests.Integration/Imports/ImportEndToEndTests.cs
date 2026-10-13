using CashPrism.Application.Bookings;
using CashPrism.Application.CashFlow;
using CashPrism.Application.Contracts;
using CashPrism.Application.Imports;
using CashPrism.Application.Paging;
using CashPrism.Domain.Bookings;
using CashPrism.Domain.Contracts;
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

    /// <summary>
    /// Two exports of the same history, the later one without an account the
    /// owner removed in Finanzguru — measured on two real exports, see
    /// <c>docs/finanzguru-export.md</c>. Every reader a page is built from is
    /// asked, because each of them would show the removed booking on its own.
    /// </summary>
    public sealed class ALaterExportThatLacksAnAccount
    {
        private const string RemovedContract = "b1c2d3e4f5a60718293a4b5c6d7e8f90";
        private const string KeptContract = "a1b2c3d4e5f60718293a4b5c6d7e8f90";

        private static readonly DateOnly March = new(2026, 3, 1);

        private static Dictionary<string, string> KeptRow()
        {
            var row = FinanzguruTestRow.Create(AFingerprint, counterparty: "Supermarkt");
            row[FinanzguruColumns.IsContract] = "ja";
            row[FinanzguruColumns.ContractInterval] = "monatlich";
            row[FinanzguruColumns.ContractId] = KeptContract;

            return row;
        }

        private static Dictionary<string, string> RemovedRow()
        {
            var row = FinanzguruTestRow.Create(AnotherFingerprint, amount: "-25.00", counterparty: "Altbank");
            row[FinanzguruColumns.AccountReference] = "DE02120300000000909090";
            row[FinanzguruColumns.AccountName] = "Altes Konto";
            row[FinanzguruColumns.IsContract] = "ja";
            row[FinanzguruColumns.ContractInterval] = "monatlich";
            row[FinanzguruColumns.ContractId] = RemovedContract;

            return row;
        }

        private static byte[] Export(string exportedOn, params Dictionary<string, string>[] rows)
            => XlsxTestWorkbook.Build(FinanzguruColumns.All, rows, sheetName: $"{exportedOn}_Export_Alle_Buchungen");

        private static async Task<CashPrismWebApplicationFactory> StartWithBothExportsAsync()
        {
            var factory = StartHost();
            await ImportAsync(factory, Export("20261009", KeptRow(), RemovedRow()));
            await ImportAsync(factory, Export("20261012", KeptRow()));

            return factory;
        }

        private static async Task<T> AskAsync<TReader, T>(
            CashPrismWebApplicationFactory factory,
            Func<TReader, Task<T>> ask)
            where TReader : notnull
        {
            await using var scope = factory.Services.CreateAsyncScope();

            return await ask(scope.ServiceProvider.GetRequiredService<TReader>());
        }

        [Fact]
        public async Task Leaves_Its_Bookings_Out_Of_The_Booking_List()
        {
            using var factory = await StartWithBothExportsAsync();

            var page = await AskAsync<IBookingReader, Page<Booking>>(
                factory,
                reader => reader.ReadPageAsync(new BookingPageRequest(0, 25)));

            Assert.Equal([AFingerprint], page.Items.Select(booking => booking.Fingerprint));
        }

        [Fact]
        public async Task Leaves_Its_Bookings_Out_Of_The_Sums()
        {
            using var factory = await StartWithBothExportsAsync();

            var summary = await AskAsync<IBookingReader, BookingSummary>(
                factory,
                reader => reader.ReadSummaryAsync(BookingFilter.None));

            Assert.Equal(new BookingSummary(1, 0, 6317), summary);
        }

        [Fact]
        public async Task Leaves_Its_Bookings_Out_Of_The_Overview_And_The_Analysis()
        {
            using var factory = await StartWithBothExportsAsync();

            var spending = await AskAsync<ICashFlowReader, IReadOnlyList<CounterpartySpending>>(
                factory,
                reader => reader.ReadCounterpartySpendingAsync(March, March));
            var nets = await AskAsync<ICashFlowReader, IReadOnlyList<CategoryNet>>(
                factory,
                reader => reader.ReadCategoryNetsAsync(March, March));

            Assert.Equal(["Supermarkt"], spending.Select(entry => entry.Counterparty));
            Assert.Equal(-6317, nets.Sum(net => net.NetInCents));
        }

        [Fact]
        public async Task Leaves_Its_Contract_Out_Of_The_Contracts()
        {
            using var factory = await StartWithBothExportsAsync();

            var contracts = await AskAsync<IContractReader, IReadOnlyList<Contract>>(
                factory,
                reader => reader.ReadAllAsync());

            Assert.Equal([KeptContract], contracts.Select(contract => contract.ContractId));
        }

        [Fact]
        public async Task Does_Not_Get_Them_Back_From_An_Older_Export()
        {
            using var factory = await StartWithBothExportsAsync();

            await ImportAsync(factory, Export("20261008", KeptRow(), RemovedRow()));

            var fingerprints = await ReadAsync(
                factory,
                context => context.Bookings.Select(booking => booking.Fingerprint).ToListAsync());
            Assert.Equal([AFingerprint], fingerprints);
        }
    }
}
