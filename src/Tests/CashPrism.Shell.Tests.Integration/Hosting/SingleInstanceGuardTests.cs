using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

/// <summary>
/// The guard that keeps a second process off a database already in use. It is
/// what stands between two writers and a corrupted SQLite file.
/// </summary>
public sealed class SingleInstanceGuardTests
{
    public sealed class TryAcquire : IDisposable
    {
        private readonly string directory = Path.Combine(
            Path.GetTempPath(),
            "cashprism-tests",
            Guid.NewGuid().ToString("n"));

        public TryAcquire()
        {
            Directory.CreateDirectory(directory);
        }

        public void Dispose()
        {
            Directory.Delete(directory, recursive: true);
        }

        private string DatabaseFile(string name = "cashprism.db")
        {
            return Path.Combine(directory, name);
        }

        [Fact]
        public void Grants_The_Guard_When_Nobody_Holds_The_Database()
        {
            using var guard = SingleInstanceGuard.TryAcquire(DatabaseFile());

            Assert.NotNull(guard);
        }

        [Fact]
        public void Refuses_The_Guard_While_The_Same_Database_Is_Held()
        {
            using var held = SingleInstanceGuard.TryAcquire(DatabaseFile());

            using var second = SingleInstanceGuard.TryAcquire(DatabaseFile());

            Assert.Null(second);
        }

        [Fact]
        public void Grants_The_Guard_For_A_Different_Database()
        {
            using var held = SingleInstanceGuard.TryAcquire(DatabaseFile());

            using var other = SingleInstanceGuard.TryAcquire(DatabaseFile("other.db"));

            Assert.NotNull(other);
        }

        [Fact]
        public void Grants_The_Guard_Again_Once_The_Holder_Released_It()
        {
            SingleInstanceGuard.TryAcquire(DatabaseFile())!.Dispose();

            using var second = SingleInstanceGuard.TryAcquire(DatabaseFile());

            Assert.NotNull(second);
        }

        [Fact]
        public void Leaves_No_Lock_File_Behind_Once_Released()
        {
            SingleInstanceGuard.TryAcquire(DatabaseFile())!.Dispose();

            Assert.False(File.Exists(DatabaseFile() + SingleInstanceGuard.LockFileSuffix));
        }
    }
}
