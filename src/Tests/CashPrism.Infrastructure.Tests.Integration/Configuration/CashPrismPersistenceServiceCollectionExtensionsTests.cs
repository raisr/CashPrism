using System.Collections.Concurrent;
using CashPrism.Application.Persistence;
using CashPrism.Infrastructure.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CashPrism.Infrastructure.Tests.Integration.Configuration;

public sealed class CashPrismPersistenceServiceCollectionExtensionsTests
{
    public sealed class AddCashPrismPersistence : IDisposable
    {
        private readonly string databaseFile = Path.Combine(
            Path.GetTempPath(),
            "cashprism-tests",
            $"{Guid.NewGuid():n}.db");

        public void Dispose()
        {
            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                SqliteConnection.ClearPool(connection);
            }

            if (File.Exists(databaseFile))
            {
                File.Delete(databaseFile);
            }
        }

        [Fact]
        public async Task Logs_The_Non_Transactional_Migration_Step_Below_Warning()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(databaseFile)!);
            var log = new RecordingLoggerProvider();

            var services = new ServiceCollection()
                .AddLogging(logging => logging.SetMinimumLevel(LogLevel.Trace).AddProvider(log))
                .AddCashPrismPersistence(databaseFile);

            await using (var provider = services.BuildServiceProvider())
            await using (var scope = provider.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync();
            }

            var levels = log.Entries
                .Where(entry => entry.EventId == RelationalEventId.NonTransactionalMigrationOperationWarning.Id)
                .Select(entry => entry.Level)
                .ToList();
            Assert.NotEmpty(levels);
            Assert.All(levels, level => Assert.Equal(LogLevel.Information, level));
        }
    }

    /// <summary>Keeps the event and the level of every entry logged.</summary>
    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(int EventId, LogLevel Level)> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(Entries);

        public void Dispose()
        {
        }

        private sealed class RecordingLogger(ConcurrentQueue<(int EventId, LogLevel Level)> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
                => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
                => entries.Enqueue((eventId.Id, logLevel));
        }
    }
}
