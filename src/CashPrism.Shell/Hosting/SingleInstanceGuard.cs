namespace CashPrism.Shell.Hosting;

/// <summary>
/// Keeps a second CashPrism process off a database file that is already in use.
/// SQLite tolerates concurrent writers badly, so the second start is refused
/// rather than left to corrupt the file.
/// </summary>
/// <remarks>
/// The guard is an exclusively opened lock file next to the database, not a
/// named mutex: an operating system closes the handles of a process that dies,
/// whichever way it died, so a crash leaves nothing behind to clean up. A named
/// mutex would need the abandoned-mutex case handled, and handles it differently
/// on each platform. The database file itself cannot carry the lock — SQLite has
/// to be able to open it.
/// </remarks>
public sealed class SingleInstanceGuard : IDisposable
{
    /// <summary>Appended to the database file name to name the lock file.</summary>
    public const string LockFileSuffix = ".lock";

    private readonly FileStream file;

    private SingleInstanceGuard(FileStream file)
    {
        this.file = file;
    }

    /// <summary>
    /// Takes the guard for <paramref name="databaseFile"/>, or returns
    /// <see langword="null"/> when another process holds it. The directory of
    /// <paramref name="databaseFile"/> must exist.
    /// </summary>
    public static SingleInstanceGuard? TryAcquire(string databaseFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);

        try
        {
            // DeleteOnClose keeps the data directory free of a file that means
            // nothing once the process is gone. A machine that loses power leaves
            // it behind, which is harmless: it is the open handle that excludes,
            // not the file's existence.
            var file = new FileStream(
                databaseFile + LockFileSuffix,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 1,
                FileOptions.DeleteOnClose);

            return new SingleInstanceGuard(file);
        }
        catch (IOException)
        {
            // A sharing violation is the expected answer, so it is a result and
            // not an exception to the caller. Anything the file system refuses
            // for another reason — no permission on the directory, a read-only
            // disk — is not this method's business and keeps travelling.
            return null;
        }
    }

    /// <summary>Releases the guard, so the next process can take it.</summary>
    public void Dispose()
    {
        file.Dispose();
    }
}
