using System.Data.Common;
using CashPrism.Application.Bookings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CashPrism.Infrastructure.Persistence;

/// <summary>
/// Gives every connection the functions in <see cref="SearchFunctions"/>.
/// SQLite keeps a function per connection, not per file, so it is registered
/// each time a connection opens — whichever host, migration or test opened it.
/// </summary>
public sealed class SearchFunctionsInterceptor : DbConnectionInterceptor
{
    /// <inheritdoc />
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        Register(connection);

        base.ConnectionOpened(connection, eventData);
    }

    /// <inheritdoc />
    public override Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Register(connection);

        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void Register(DbConnection connection)
    {
        if (connection is SqliteConnection sqlite)
        {
            sqlite.CreateFunction<string?, string?>(
                SearchFunctions.FoldName,
                text => text is null ? null : SearchText.Fold(text),
                isDeterministic: true);
        }
    }
}
