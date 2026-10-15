using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Builds a context for the EF Core command-line tools, so
/// <c>dotnet ef migrations add</c> works against this project alone.
/// </summary>
/// <remarks>
/// Without it the tools would have to start the application to find a context, and
/// the composition root would then be a build-time dependency of the migrations.
/// The connection string below names a file that is never opened: adding a
/// migration reads the model, it does not touch a database. The application's own
/// path comes from the host through
/// <c>AddCashPrismPersistence</c>.
/// </remarks>
public sealed class CashPrismDbContextFactory : IDesignTimeDbContextFactory<CashPrismDbContext>
{
    /// <inheritdoc />
    public CashPrismDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CashPrismDbContext>()
            .UseSqlite("Data Source=cashprism.design-time.db")
            .Options;

        return new CashPrismDbContext(options);
    }
}
