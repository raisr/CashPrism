namespace CashPrism.Application.Persistence;

/// <summary>
/// Brings the database to the schema this build expects. The host calls it once
/// before the first request is served.
/// </summary>
/// <remarks>
/// It exists so the host can migrate without knowing what the database is: the
/// schema, the migrations and the provider stay inside the infrastructure layer.
/// CashPrism ships as one executable a person double-clicks, so there is no
/// separate deployment step this could happen in instead.
/// </remarks>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Applies every migration the database is missing. Does nothing when it is
    /// already current, and creates the database when it does not exist yet.
    /// </summary>
    /// <param name="cancellationToken">Cancels the migration.</param>
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
