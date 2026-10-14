using System.Diagnostics;
using System.Text;
using CashPrism.Shell.Hosting;

namespace CashPrism.Shell.Tests.Integration.Hosting;

/// <summary>
/// The executable, started as a real process against a throwaway data directory.
/// What a second instance does cannot be faked in-process: the guard under test
/// is an operating-system file handle, and only a real process can be killed to
/// see it released.
/// </summary>
internal sealed class ShellProcess : IDisposable
{
    /// <summary>The banner's last line, printed once the server listens.</summary>
    private const string ListeningMarker = "Press Ctrl+C to stop.";

    /// <summary>What the host logs in front of its content root on start.</summary>
    private const string ContentRootMarker = "Content root path: ";

    /// <summary>
    /// How long a killed process may keep its files open after it was reported
    /// exited. Windows closes the handles of a dead process a moment after the
    /// exit is signalled — measured at up to 23 ms, in about half of 90 kills —
    /// so the files are free soon after, not at once. A ceiling, not an expected
    /// duration.
    /// </summary>
    private static readonly TimeSpan ReleasePatience = TimeSpan.FromSeconds(5);

    private readonly TaskCompletionSource listening =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource<string> contentRoot =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly StringBuilder error = new();

    private readonly StringBuilder output = new();

    private readonly Process process;

    private readonly string dataDirectory;

    private ShellProcess(Process process, string dataDirectory)
    {
        this.process = process;
        this.dataDirectory = dataDirectory;
    }

    /// <summary>Completes once the process has printed that it is listening.</summary>
    public Task Listening => listening.Task;

    /// <summary>Completes with the content root the host reports, once it has.</summary>
    public Task<string> ContentRoot => contentRoot.Task;

    /// <summary>Whether the process is still running.</summary>
    public bool IsRunning => !process.HasExited;

    /// <summary>The exit code. Only meaningful once the process has exited.</summary>
    public int ExitCode => process.ExitCode;

    /// <summary>Everything the process has written to standard output so far.</summary>
    public string Output
    {
        get
        {
            lock (output)
            {
                return output.ToString();
            }
        }
    }

    /// <summary>Everything the process has written to standard error so far.</summary>
    public string Error
    {
        get
        {
            lock (error)
            {
                return error.ToString();
            }
        }
    }

    /// <summary>
    /// Starts the executable against <paramref name="dataDirectory"/>. The port
    /// is left to the operating system — this is about the database, and a fixed
    /// port would collide with whatever else the machine happens to run.
    /// </summary>
    /// <param name="dataDirectory">The data directory to start against.</param>
    /// <param name="workingDirectory">
    /// The directory to start in. Defaults to the one holding the executable,
    /// which is what a double-click on Windows does.
    /// </param>
    /// <param name="arguments">Further switches, as a person would type them.</param>
    public static ShellProcess Start(
        string dataDirectory,
        string? workingDirectory = null,
        IEnumerable<string>? arguments = null)
    {
        var startInfo = new ProcessStartInfo(Executable())
        {
            ArgumentList =
            {
                "--port",
                "0",
                HostingCommandLine.NoBrowserSwitch,
                $"--{HostingOptions.SectionName}:{nameof(HostingOptions.DataDirectory)}={dataDirectory}",

                // Loopback only. The application binds every interface on purpose,
                // and Windows answers that with a firewall prompt for every new
                // path of the executable — a dialog no test run may wait on.
                // Endpoints from the Kestrel section take precedence over the
                // address Program sets, so the executable itself stays as shipped.
                "--Kestrel:Endpoints:Http:Url=http://127.0.0.1:0",
            },
            WorkingDirectory = workingDirectory ?? AppContext.BaseDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in arguments ?? [])
        {
            startInfo.ArgumentList.Add(argument);
        }

        var process = new Process { StartInfo = startInfo };
        var started = new ShellProcess(process, dataDirectory);

        // Both streams are drained: a process whose output nobody reads blocks on
        // a full pipe, and the banner is longer than one.
        process.OutputDataReceived += (_, e) => started.OnOutput(e.Data);
        process.ErrorDataReceived += (_, e) => started.OnError(e.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return started;
    }

    /// <summary>Waits for the process to exit, or fails after a generous timeout.</summary>
    public async Task WaitForExitAsync(TimeSpan patience)
    {
        using var cancellation = new CancellationTokenSource(patience);

        await process.WaitForExitAsync(cancellation.Token);
    }

    /// <summary>
    /// Ends the process the way a crash would: no shutdown, no chance to clean
    /// anything up. Returns once the files in the data directory are free again,
    /// or once <see cref="ReleasePatience"/> has passed — whatever then still
    /// holds them is left for the test to find.
    /// </summary>
    public async Task KillAsync(TimeSpan patience)
    {
        process.Kill(entireProcessTree: true);

        await WaitForExitAsync(patience);
        await WaitForFilesReleasedAsync();
    }

    public void Dispose()
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
        }

        process.Dispose();
    }

    private async Task WaitForFilesReleasedAsync()
    {
        var deadline = DateTime.UtcNow + ReleasePatience;

        while (AnyFileHeld() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }

    private bool AnyFileHeld()
    {
        if (!Directory.Exists(dataDirectory))
        {
            return false;
        }

        return Directory.EnumerateFiles(dataDirectory).Any(IsHeld);
    }

    private static bool IsHeld(string file)
    {
        try
        {
            using var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private static string Executable()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            OperatingSystem.IsWindows() ? "CashPrism.Shell.exe" : "CashPrism.Shell");
    }

    private void OnOutput(string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (output)
        {
            output.AppendLine(line);
        }

        var contentRootAt = line.IndexOf(ContentRootMarker, StringComparison.Ordinal);

        if (contentRootAt >= 0)
        {
            contentRoot.TrySetResult(line[(contentRootAt + ContentRootMarker.Length)..].Trim());
        }

        if (line.Contains(ListeningMarker, StringComparison.Ordinal))
        {
            listening.TrySetResult();
        }
    }

    private void OnError(string? line)
    {
        if (line is null)
        {
            return;
        }

        lock (error)
        {
            error.AppendLine(line);
        }
    }
}
