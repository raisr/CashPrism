using CashPrism.Application.Imports;
using CashPrism.Infrastructure.Finanzguru;
using CashPrism.Infrastructure.Persistence;
using CashPrism.TestSupport.Imports;
using CashPrism.TestSupport.Samples;

namespace CashPrism.Infrastructure.Tests.Integration;

/// <summary>
/// A <see cref="ThrowawayDatabase"/> holding the committed demo export,
/// imported the way the application imports it. Expected figures asserted
/// against it are measured from the workbook itself, outside CashPrism.
/// </summary>
public static class DemoSampleDatabase
{
    /// <summary>When the demo export is imported: the day after its export date.</summary>
    public static readonly DateTimeOffset ImportedAt = new(2026, 10, 2, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Creates the database and imports the demo export into it.</summary>
    public static async Task<ThrowawayDatabase> CreateAsync()
    {
        var database = await ThrowawayDatabase.CreateAsync();

        await ImportAsync(database);

        return database;
    }

    /// <summary>Imports the demo export into <paramref name="database"/>, again or for the first time.</summary>
    /// <param name="database">The database to import into.</param>
    public static async Task ImportAsync(ThrowawayDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        await using var context = database.CreateContext();
        await using var stream = DemoSample.Open();

        var importer = new Importer(
            new FinanzguruImportSource(),
            new ImportStore(context),
            new FixedClock(ImportedAt));

        await importer.ImportAsync(stream, DemoSample.FileName);
    }
}
