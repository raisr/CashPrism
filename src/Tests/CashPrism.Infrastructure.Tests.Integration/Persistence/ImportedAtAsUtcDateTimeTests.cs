using CashPrism.Application.Paging;
using CashPrism.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The migration that rewrites <c>ImportRuns.ImportedAt</c> from the text a
/// <c>DateTimeOffset</c> is stored as to the text a UTC <c>DateTime</c> is
/// stored as. It is written by hand, because the column type does not change
/// and EF Core therefore generated nothing — so it is also the migration that
/// most needs a test run against rows that were already there.
/// </summary>
public sealed class ImportedAtAsUtcDateTimeTests
{
    /// <summary>The migration before this one, by name without its timestamp.</summary>
    private const string TheVersionBefore = "ImportCountsAndBookingSource";

    /// <summary>
    /// Writes a row the way the version before this migration wrote it: the
    /// instant as text with the offset appended. It has to be raw SQL, because
    /// this build's configuration would write the new format.
    /// </summary>
    private static async Task InsertAsWrittenBeforeAsync(
        ThrowawayDatabase database,
        Guid id,
        string importedAt,
        string fileHash)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO ImportRuns
                (Id, FileName, FileHash, SheetName, ExportedOn, ImportedAt,
                 RowsRead, BookingsInserted, BookingsUpdated, BookingsUnchanged, IsComplete)
            VALUES
                ($id, 'export.xlsx', $hash, '20260907_Export_Alle_Buchungen', '2026-09-07', $importedAt,
                 6327, 12, 3, 6312, 1);
            """;
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$hash", fileHash);
        command.Parameters.AddWithValue("$importedAt", importedAt);

        await command.ExecuteNonQueryAsync();
    }

    public sealed class TheMigration
    {
        [Fact]
        public async Task Leaves_An_Instant_Written_Before_It_Unchanged()
        {
            await using var database = await ThrowawayDatabase.CreateAsync(TheVersionBefore);
            await InsertAsWrittenBeforeAsync(
                database,
                Guid.NewGuid(),
                "2026-09-07 18:04:33.1234567+00:00",
                "3b8f1c");

            await database.MigrateAsync();

            await using var context = database.CreateContext();
            var run = await context.ImportRuns.SingleAsync();

            Assert.Equal(
                new DateTimeOffset(2026, 9, 7, 18, 4, 33, TimeSpan.Zero).AddTicks(1_234_567),
                run.ImportedAt);
        }

        [Fact]
        public async Task Keeps_The_Instant_A_Utc_One()
        {
            await using var database = await ThrowawayDatabase.CreateAsync(TheVersionBefore);
            await InsertAsWrittenBeforeAsync(
                database,
                Guid.NewGuid(),
                "2026-09-07 18:04:33+00:00",
                "3b8f1c");

            await database.MigrateAsync();

            await using var context = database.CreateContext();

            Assert.Equal(TimeSpan.Zero, (await context.ImportRuns.SingleAsync()).ImportedAt.Offset);
        }

        [Fact]
        public async Task Drops_The_Offset_From_What_The_Column_Holds()
        {
            await using var database = await ThrowawayDatabase.CreateAsync(TheVersionBefore);
            await InsertAsWrittenBeforeAsync(
                database,
                Guid.NewGuid(),
                "2026-09-07 18:04:33+00:00",
                "3b8f1c");

            await database.MigrateAsync();

            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT ImportedAt FROM ImportRuns";

            Assert.Equal("2026-09-07 18:04:33", await command.ExecuteScalarAsync());
        }

        /// <summary>
        /// The point of the whole change: rows written before it can be ordered
        /// by the database afterwards. With the offset still appended, SQLite
        /// would not be asked to order them at all.
        /// </summary>
        [Fact]
        public async Task Lets_The_Database_Order_Rows_Written_Before_It()
        {
            await using var database = await ThrowawayDatabase.CreateAsync(TheVersionBefore);
            var older = Guid.NewGuid();
            var newer = Guid.NewGuid();
            await InsertAsWrittenBeforeAsync(database, older, "2026-09-07 18:04:33+00:00", "3b8f1c");
            await InsertAsWrittenBeforeAsync(database, newer, "2026-09-08 09:15:02+00:00", "9d0d23");

            await database.MigrateAsync();

            await using var context = database.CreateContext();
            var reader = new ImportRunReader(context);

            var page = await reader.ReadPageAsync(new PageRequest(Skip: 0, Take: 25));

            Assert.Equal([newer, older], page.Items.Select(run => run.Id));
        }

        /// <summary>
        /// Every offset the application's clock ever wrote is <c>+00:00</c>. A row
        /// carrying another one is left alone rather than shifted by however many
        /// hours it was written with — it is then unreadable, which is the point:
        /// loud beats wrong.
        /// </summary>
        [Fact]
        public async Task Leaves_A_Row_With_Another_Offset_Alone()
        {
            await using var database = await ThrowawayDatabase.CreateAsync(TheVersionBefore);
            await InsertAsWrittenBeforeAsync(
                database,
                Guid.NewGuid(),
                "2026-09-07 18:04:33+02:00",
                "3b8f1c");

            await database.MigrateAsync();

            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT ImportedAt FROM ImportRuns";

            Assert.Equal("2026-09-07 18:04:33+02:00", await command.ExecuteScalarAsync());
        }
    }
}
