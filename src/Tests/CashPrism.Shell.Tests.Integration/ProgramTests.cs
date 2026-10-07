using CashPrism.Shell.Tests.Integration.Hosting;

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
            var instance = Start(workingDirectory: elsewhere);

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

        private ShellProcess Start(string? workingDirectory = null)
        {
            var instance = ShellProcess.Start(DataDirectory, workingDirectory);

            started.Add(instance);

            return instance;
        }

        private async Task<ShellProcess> StartListeningAsync()
        {
            var instance = Start();

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
