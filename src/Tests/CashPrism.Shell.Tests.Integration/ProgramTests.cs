using CashPrism.Domain.Access;
using CashPrism.Domain.Imports;
using CashPrism.Infrastructure.Persistence;
using CashPrism.Shell.Hosting;
using CashPrism.Shell.Tests.Integration.Hosting;
using CashPrism.TestSupport.Bookings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CashPrism.Shell.Tests.Integration;

/// <summary>
/// The executable as a real process, where what is under test only exists in
/// one: the directory it is started in, and what a start leaves behind for the
/// next one.
/// </summary>
public sealed class ProgramTests
{
    public sealed class Main : IDisposable
    {
        /// <summary>
        /// A cold start builds the host and applies the migrations, so this is
        /// deliberately generous. It is a ceiling that turns a hang into a failing
        /// test, not an expected duration.
        /// </summary>
        private static readonly TimeSpan Patience = TimeSpan.FromSeconds(120);

        private readonly string root = Path.Combine(
            Path.GetTempPath(),
            "cashprism-tests",
            Guid.NewGuid().ToString("n"));

        private readonly List<ShellProcess> started = [];

        private string DataDirectory => Path.Combine(root, "data");

        private string KeysDirectory => Path.Combine(DataDirectory, "keys");

        public void Dispose()
        {
            foreach (var process in started)
            {
                process.Dispose();
            }

            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }

        // The content root decides where appsettings.json and wwwroot are read
        // from. A build output finds its static assets through a manifest with
        // absolute paths whatever the content root is, so only a published build
        // shows the failure: there every asset was served empty.
        [Fact]
        public async Task Takes_The_Directory_Of_The_Executable_As_Content_Root_When_Started_Elsewhere()
        {
            var elsewhere = Directory.CreateDirectory(Path.Combine(root, "elsewhere")).FullName;
            // The host reports its content root at Information, which the shipped
            // configuration keeps off the console; this start asks for it back.
            var instance = Start(workingDirectory: elsewhere, "--Logging:LogLevel:Microsoft.Hosting.Lifetime=Information");

            var finished = await Task.WhenAny(instance.ContentRoot, Task.Delay(Patience));

            Assert.True(
                finished == instance.ContentRoot,
                $"The instance never reported its content root. {instance.Error}");
            Assert.Equal(
                Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory),
                Path.TrimEndingDirectorySeparator(await instance.ContentRoot));
        }

        [Fact]
        public async Task Writes_A_Data_Protection_Key_Into_The_Data_Directory()
        {
            await StartListeningAsync();

            Assert.NotEmpty(KeyFiles());
        }

        // Every new key ring invalidates what the old one protected, so a key
        // created on every start would sign out every device on every restart.
        [Fact]
        public async Task Reuses_The_Data_Protection_Key_Of_The_Previous_Start()
        {
            var first = await StartListeningAsync();
            var keysAfterFirstStart = KeyFiles();
            await first.KillAsync(Patience);

            await StartListeningAsync();

            // Two empty directories are equal as well, and prove nothing.
            Assert.NotEmpty(keysAfterFirstStart);
            Assert.Equal(keysAfterFirstStart, KeyFiles());
        }

        [Fact]
        public async Task Shows_A_Setup_Code_On_A_Fresh_Database()
        {
            var instance = await StartListeningAsync();

            Assert.Contains("No password is set yet.", instance.Output, StringComparison.Ordinal);
        }

        // The banner reads the database on every start, so a password set in
        // between is what decides — not anything the first start remembered.
        [Fact]
        public async Task Shows_No_Setup_Code_Once_A_Password_Is_Set()
        {
            var first = await StartListeningAsync();
            await first.KillAsync(Patience);
            await StoreACredentialAsync();

            var second = await StartListeningAsync();

            Assert.DoesNotContain("No password is set yet.", second.Output, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Shows_A_Setup_Code_Again_When_Started_To_Reset_The_Password()
        {
            await PrepareAsync();
            await StoreACredentialAsync();

            var instance = await StartListeningAsync(HostingCommandLine.ResetPasswordSwitch);

            Assert.Contains("No password is set yet.", instance.Output, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Drops_The_Stored_Password_When_Started_To_Reset_It()
        {
            await PrepareAsync();
            await StoreACredentialAsync();

            var instance = await StartListeningAsync(HostingCommandLine.ResetPasswordSwitch);
            await instance.KillAsync(Patience);

            Assert.Equal(false, (await ReadCredentialAsync())?.IsPasswordSet);
        }

        [Fact]
        public async Task Keeps_Every_Booking_And_Import_When_Started_To_Reset_The_Password()
        {
            await PrepareAsync();
            await StoreACredentialAsync();
            await StoreAnImportAsync();

            var instance = await StartListeningAsync(HostingCommandLine.ResetPasswordSwitch);
            await instance.KillAsync(Patience);

            Assert.Equal((1, 1), await CountBookingsAndImportsAsync());
        }

        // A database exists only once a start has migrated it; what a test stores
        // before the start under test goes into that one.
        private async Task PrepareAsync()
        {
            var first = await StartListeningAsync();
            await first.KillAsync(Patience);
        }

        private string ConnectionString => $"Data Source={Path.Combine(DataDirectory, "cashprism.db")}";

        private CashPrismDbContext CreateContext()
            => new(new DbContextOptionsBuilder<CashPrismDbContext>()
                .UseSqlite(ConnectionString)
                .Options);

        /// <summary>
        /// Lets go of the file the pool keeps open, so the next start may take
        /// it. Only this database's pool: clearing every pool in the process
        /// would close the connections of tests running beside this one.
        /// </summary>
        private void ReleaseDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            SqliteConnection.ClearPool(connection);
        }

        private async Task StoreAnImportAsync()
        {
            await using (var context = CreateContext())
            {
                context.ImportRuns.Add(new ImportRun(
                    TestBookings.AnImportRunId,
                    "export.xlsx",
                    "3b8f1c",
                    "20261005_Export_Alle_Buchungen",
                    new DateOnly(2026, 10, 5),
                    DateTimeOffset.UnixEpoch));
                context.Bookings.Add(TestBookings.Create());
                await context.SaveChangesAsync();
            }

            ReleaseDatabase();
        }

        private async Task<Credential?> ReadCredentialAsync()
        {
            Credential? credential;

            await using (var context = CreateContext())
            {
                credential = await new CredentialStore(context).GetAsync();
            }

            ReleaseDatabase();

            return credential;
        }

        private async Task<(int Bookings, int Imports)> CountBookingsAndImportsAsync()
        {
            (int, int) counts;

            await using (var context = CreateContext())
            {
                counts = (await context.Bookings.CountAsync(), await context.ImportRuns.CountAsync());
            }

            ReleaseDatabase();

            return counts;
        }

        private async Task StoreACredentialAsync()
        {
            await using (var context = CreateContext())
            {
                await new CredentialStore(context).AddAsync(new Credential("a-hash", DateTimeOffset.UnixEpoch));
            }

            ReleaseDatabase();
        }

        private ShellProcess Start(string? workingDirectory = null, params string[] arguments)
        {
            var instance = ShellProcess.Start(DataDirectory, workingDirectory, arguments);

            started.Add(instance);

            return instance;
        }

        private async Task<ShellProcess> StartListeningAsync(params string[] arguments)
        {
            var instance = Start(arguments: arguments);

            var finished = await Task.WhenAny(instance.Listening, Task.Delay(Patience));

            Assert.True(
                finished == instance.Listening,
                $"The instance never reported that it was listening. {instance.Error}");

            return instance;
        }

        private string[] KeyFiles()
        {
            return Directory.Exists(KeysDirectory)
                ? [.. Directory.GetFiles(KeysDirectory, "key-*.xml").Order(StringComparer.Ordinal)]
                : [];
        }
    }
}
