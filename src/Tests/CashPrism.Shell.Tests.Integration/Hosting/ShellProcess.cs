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

    private readonly TaskCompletionSource listening =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly StringBuilder error = new();

    private readonly Process process;

    private ShellProcess(Process process)
    {
        this.process = process;
    }

    /// <summary>Completes once the process has printed that it is listening.</summary>
    public Task Listening => listening.Task;

    /// <summary>Whether the process is still running.</summary>
    public bool IsRunning => !process.HasExited;

    /// <summary>The exit code. Only meaningful once the process has exited.</summary>
    public int ExitCode => process.ExitCode;

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
    public static ShellProcess Start(string dataDirectory)
    {
        var startInfo = new ProcessStartInfo(Executable())
        {
            ArgumentList =
            {
                "--port",
                "0",
                HostingCommandLine.NoBrowserSwitch,
                $"--{HostingOptions.SectionName}:{nameof(HostingOptions.DataDirectory)}={dataDirectory}",
            },
            WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var process = new Process { StartInfo = startInfo };
        var started = new ShellProcess(process);

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
    /// anything up.
    /// </summary>
    public async Task KillAsync(TimeSpan patience)
    {
        process.Kill(entireProcessTree: true);

        await WaitForExitAsync(patience);
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

    private static string Executable()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            OperatingSystem.IsWindows() ? "CashPrism.Shell.exe" : "CashPrism.Shell");
    }

    private void OnOutput(string? line)
    {
        if (line?.Contains(ListeningMarker, StringComparison.Ordinal) == true)
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
