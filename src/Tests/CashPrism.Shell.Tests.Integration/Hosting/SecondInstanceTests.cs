using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

/// <summary>
/// Two real processes against one data directory. <see cref="SingleInstanceGuardTests"/>
/// covers the guard itself; this covers that the executable is wired to it, says
/// what happened, and lets go of it when it dies.
/// </summary>
public sealed class SecondInstanceTests
{
    public sealed class Startup : IDisposable
    {
        /// <summary>
        /// A cold start builds the host and applies the migrations, so this is
        /// deliberately generous. It is a ceiling that turns a hang into a failing
        /// test, not an expected duration.
        /// </summary>
        private static readonly TimeSpan Patience = TimeSpan.FromSeconds(120);

        private readonly List<string> directories = [];

        private readonly List<ShellProcess> started = [];

        private readonly string dataDirectory;

        public Startup()
        {
            dataDirectory = NewDataDirectory();
        }

        public void Dispose()
        {
            foreach (var process in started)
            {
                process.Dispose();
            }

            foreach (var directory in directories.Where(Directory.Exists))
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        private string DatabaseFile => Path.Combine(dataDirectory, "cashprism.db");

        private string NewDataDirectory()
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "cashprism-tests",
                Guid.NewGuid().ToString("n"));

            directories.Add(directory);

            return directory;
        }

        private ShellProcess Start(string? directory = null)
        {
            var instance = ShellProcess.Start(directory ?? dataDirectory);

            started.Add(instance);

            return instance;
        }

        private async Task<ShellProcess> StartListeningInstanceAsync(string? directory = null)
        {
            var instance = Start(directory);

            var finished = await Task.WhenAny(instance.Listening, Task.Delay(Patience));

            Assert.True(
                finished == instance.Listening,
                $"An instance never reported that it was listening. {instance.Error}");

            return instance;
        }

        [Fact]
        public async Task A_Second_Instance_On_The_Same_Database_Exits_With_A_Failure()
        {
            await StartListeningInstanceAsync();

            var second = Start();
            await second.WaitForExitAsync(Patience);

            Assert.NotEqual(0, second.ExitCode);
        }

        [Fact]
        public async Task A_Refused_Second_Instance_Names_The_Database_It_Could_Not_Have()
        {
            await StartListeningInstanceAsync();

            var second = Start();
            await second.WaitForExitAsync(Patience);

            Assert.Contains(DatabaseFile, second.Error, StringComparison.Ordinal);
        }

        [Fact]
        public async Task The_First_Instance_Keeps_Running_When_A_Second_Is_Refused()
        {
            var first = await StartListeningInstanceAsync();

            var second = Start();
            await second.WaitForExitAsync(Patience);

            Assert.True(first.IsRunning);
        }

        [Fact]
        public async Task A_Second_Instance_On_Another_Database_Starts_As_Well()
        {
            await StartListeningInstanceAsync();

            var second = await StartListeningInstanceAsync(NewDataDirectory());

            Assert.True(second.IsRunning);
        }

        [Fact]
        public async Task The_Database_Is_Free_Again_Once_The_Owning_Process_Is_Killed()
        {
            var first = await StartListeningInstanceAsync();

            await first.KillAsync(Patience);

            using var guard = SingleInstanceGuard.TryAcquire(DatabaseFile);
            Assert.NotNull(guard);
        }
    }
}
