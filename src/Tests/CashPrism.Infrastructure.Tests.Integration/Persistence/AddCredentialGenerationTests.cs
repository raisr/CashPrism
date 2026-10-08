using CashPrism.Infrastructure.Persistence;

namespace CashPrism.Infrastructure.Tests.Integration.Persistence;

/// <summary>
/// The migration that makes the password hash optional and adds the generation.
/// SQLite cannot relax a column in place, so EF Core rebuilds the table — and a
/// password lost on the way would lock the household out of an upgraded
/// CashPrism.
/// </summary>
public sealed class AddCredentialGenerationTests
{
    /// <summary>The migration before this one, by name without its timestamp.</summary>
    private const string TheVersionBefore = "AddCredentials";

    /// <summary>
    /// Writes the credential the way the version before this migration wrote
    /// it. Raw SQL, because this build's model has a column that table lacks.
    /// </summary>
    private static async Task InsertAsWrittenBeforeAsync(ThrowawayDatabase database, string passwordHash)
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Credentials (Id, PasswordHash, SetAt)
            VALUES (1, $hash, '2026-10-07 20:00:00');
            """;
        command.Parameters.AddWithValue("$hash", passwordHash);

        await command.ExecuteNonQueryAsync();
    }

    public sealed class TheMigration
    {
        [Fact]
        public async Task Keeps_A_Password_Set_Before_It()
        {
            await using var database = await ThrowawayDatabase.CreateAsync(TheVersionBefore);
            await InsertAsWrittenBeforeAsync(database, "a-hash-from-before");

            await database.MigrateAsync();

            await using var context = database.CreateContext();
            var credential = await new CredentialStore(context).GetAsync();

            Assert.Equal("a-hash-from-before", credential?.PasswordHash);
        }
    }
}
