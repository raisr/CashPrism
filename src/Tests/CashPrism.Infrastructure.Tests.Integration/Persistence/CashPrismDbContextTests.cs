using CashPrism.Domain.Bookings;
using CashPrism.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

public sealed class CashPrismDbContextTests
{
    private const string AFingerprint = "0f4c3a1b2d5e6f708192a3b4c5d6e7f809a1b2c3";
    private const string AnotherFingerprint = "1a2b3c4d5e6f708192a3b4c5d6e7f809a1b2c3d4";

    private static Booking CreateBooking(
        string fingerprint = AFingerprint,
        decimal amount = -63.17m,
        SplitRole splitRole = SplitRole.None,
        string? originalFingerprint = null)
        => new(
            fingerprint,
            new DateTime(2026, 3, 12, 9, 41, 0, DateTimeKind.Unspecified),
            amount,
            currency: "EUR",
            accountReference: "DE02120300000000202051",
            accountName: "Girokonto",
            counterparty: "Supermarkt",
            counterpartyAccount: "DE02500105170137075030",
            paymentReference: "Kartenzahlung",
            category: "Lebensmittel",
            subCategory: "Supermarkt",
            isTransfer: false,
            splitRole,
            originalFingerprint);

    private static ImportRun CreateRun(Guid id, DateOnly? exportedOn = null)
        => new(
            id,
            fileName: "20260314_Export_Alle_Buchungen.xlsx",
            fileHash: "3b8f1c",
            sheetName: "20260314_Export_Alle_Buchungen",
            exportedOn,
            importedAt: new DateTimeOffset(2026, 3, 14, 18, 0, 0, TimeSpan.FromHours(1)));

    public sealed class Bookings
    {
        [Fact]
        public async Task Come_Back_Field_For_Field()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var written = CreateBooking();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(written);
                await context.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();
            var read = await reading.Bookings.SingleAsync();

            Assert.Equal(written.Fingerprint, read.Fingerprint);
            Assert.Equal(written.BookedOn, read.BookedOn);
            Assert.Equal(written.Amount, read.Amount);
            Assert.Equal(written.Currency, read.Currency);
            Assert.Equal(written.AccountReference, read.AccountReference);
            Assert.Equal(written.AccountName, read.AccountName);
            Assert.Equal(written.Counterparty, read.Counterparty);
            Assert.Equal(written.CounterpartyAccount, read.CounterpartyAccount);
            Assert.Equal(written.PaymentReference, read.PaymentReference);
            Assert.Equal(written.Category, read.Category);
            Assert.Equal(written.SubCategory, read.SubCategory);
            Assert.Equal(written.IsTransfer, read.IsTransfer);
            Assert.Equal(written.SplitRole, read.SplitRole);
            Assert.Equal(written.OriginalFingerprint, read.OriginalFingerprint);
        }

        [Fact]
        public async Task Keep_The_Time_Of_Day_The_Export_Carries_On_Some_Rows()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(CreateBooking());
                await context.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();
            var read = await reading.Bookings.SingleAsync();

            Assert.Equal(new TimeSpan(9, 41, 0), read.BookedOn.TimeOfDay);
        }

        [Fact]
        public async Task Cannot_Be_Stored_Twice_Under_One_Fingerprint()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(CreateBooking());
                await context.SaveChangesAsync();
            }

            await using var second = database.CreateContext();
            second.Bookings.Add(CreateBooking(amount: -1m));

            await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync());
        }

        [Fact]
        public async Task Store_Their_Split_Role_By_Name_So_The_File_Can_Be_Read()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(CreateBooking(
                    splitRole: SplitRole.Remainder,
                    originalFingerprint: AnotherFingerprint));
                await context.SaveChangesAsync();
            }

            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT SplitRole FROM Bookings";

            Assert.Equal("Remainder", await command.ExecuteScalarAsync() as string);
        }
    }

    public sealed class Amounts
    {
        [Theory]
        [InlineData(-63.17)]
        [InlineData(3240.00)]
        [InlineData(0)]
        [InlineData(-0.01)]
        public async Task Survive_The_Round_Trip_To_The_Cent(decimal amount)
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(CreateBooking(amount: amount));
                await context.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();
            var read = await reading.Bookings.SingleAsync();

            Assert.Equal(amount, read.Amount);
        }

        [Fact]
        public async Task Are_Stored_As_Whole_Cents_So_Sorting_And_Summing_Work()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(CreateBooking(amount: -63.17m));
                await context.SaveChangesAsync();
            }

            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT typeof(Amount), Amount FROM Bookings";
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();

            Assert.Equal("integer", reader.GetString(0));
            Assert.Equal(-6317, reader.GetInt64(1));
        }

        [Fact]
        public async Task Are_Refused_Rather_Than_Rounded_When_They_Are_Finer_Than_A_Cent()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using var context = database.CreateContext();
            context.Bookings.Add(CreateBooking(amount: -63.175m));

            // EF wraps whatever the conversion throws, so the reason has to be read
            // from the inner exception — which is the point of asserting it here: a
            // third decimal place must not reach the database as a rounded value.
            var failure = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

            Assert.Contains("more precision than cents", failure.InnerException?.Message);
        }

        [Fact]
        public async Task Sort_By_Value_Rather_Than_By_Their_Text()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using (var context = database.CreateContext())
            {
                context.Bookings.Add(CreateBooking(fingerprint: AFingerprint, amount: -9m));
                context.Bookings.Add(CreateBooking(fingerprint: AnotherFingerprint, amount: -100m));
                await context.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();
            var amounts = await reading.Bookings
                .OrderBy(booking => booking.Amount)
                .Select(booking => booking.Amount)
                .ToListAsync();

            Assert.Equal([-100m, -9m], amounts);
        }
    }

    public sealed class ImportRuns
    {
        [Fact]
        public async Task Come_Back_With_No_Export_Date_When_The_Sheet_Name_Had_None()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var id = Guid.NewGuid();

            await using (var context = database.CreateContext())
            {
                context.ImportRuns.Add(CreateRun(id, exportedOn: null));
                await context.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();
            var read = await reading.ImportRuns.SingleAsync();

            Assert.Null(read.ExportedOn);
            Assert.Equal(id, read.Id);
        }

        [Fact]
        public async Task Keep_The_Export_Date_As_A_Date()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var exportedOn = new DateOnly(2026, 3, 14);

            await using (var context = database.CreateContext())
            {
                context.ImportRuns.Add(CreateRun(Guid.NewGuid(), exportedOn));
                await context.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();
            var read = await reading.ImportRuns.SingleAsync();

            Assert.Equal(exportedOn, read.ExportedOn);
        }
    }

    public sealed class RawRows
    {
        [Fact]
        public async Task Are_Kept_Once_Per_Run_And_Booking()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var runId = Guid.NewGuid();

            await using (var context = database.CreateContext())
            {
                context.ImportRuns.Add(CreateRun(runId));
                context.RawRows.Add(new RawRow(runId, AFingerprint, """{"Betrag":-63.17}"""));
                await context.SaveChangesAsync();
            }

            await using var second = database.CreateContext();
            second.RawRows.Add(new RawRow(runId, AFingerprint, """{"Betrag":-1.00}"""));

            await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync());
        }

        [Fact]
        public async Task Need_The_Run_They_Were_Read_By()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();

            await using var context = database.CreateContext();
            context.RawRows.Add(new RawRow(Guid.NewGuid(), AFingerprint, """{"Betrag":-63.17}"""));

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        [Fact]
        public async Task Go_With_The_Run_When_It_Is_Deleted()
        {
            await using var database = await ThrowawayDatabase.CreateAsync();
            var runId = Guid.NewGuid();

            await using (var context = database.CreateContext())
            {
                context.ImportRuns.Add(CreateRun(runId));
                context.RawRows.Add(new RawRow(runId, AFingerprint, """{"Betrag":-63.17}"""));
                await context.SaveChangesAsync();
            }

            await using (var deleting = database.CreateContext())
            {
                deleting.ImportRuns.Remove(await deleting.ImportRuns.SingleAsync());
                await deleting.SaveChangesAsync();
            }

            await using var reading = database.CreateContext();

            Assert.Empty(await reading.RawRows.ToListAsync());
        }
    }
}
