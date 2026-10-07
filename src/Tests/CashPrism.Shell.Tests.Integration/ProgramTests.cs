using CashPrism.Shell.Tests.Integration.Hosting;

namespace CashPrism.Shell.Tests.Integration;

/// <summary>
/// The executable as a real process, where what is under test only exists in
/// one: the directory it is started in.
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

        private ShellProcess? started;

        public void Dispose()
        {
            started?.Dispose();

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
            started = ShellProcess.Start(Path.Combine(root, "data"), workingDirectory: elsewhere);

            var finished = await Task.WhenAny(started.ContentRoot, Task.Delay(Patience));

            Assert.True(
                finished == started.ContentRoot,
                $"The instance never reported its content root. {started.Error}");
            Assert.Equal(
                Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory),
                Path.TrimEndingDirectorySeparator(await started.ContentRoot));
        }
    }
}
