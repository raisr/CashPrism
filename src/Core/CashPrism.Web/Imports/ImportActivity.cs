using CashPrism.Web.StoredData;

namespace CashPrism.Web.Imports;

/// <summary>
/// Whether an import is running, and what the last one said. Registered per
/// circuit rather than held in the page, because a page does not survive
/// navigating away from it and an import does.
/// </summary>
/// <remarks>
/// Without this, leaving the Import page and coming back gives a fresh
/// component with no idea that work is still in flight: it shows no spinner and
/// lets a second import start on top of the first. The state therefore lives
/// beside the page, for as long as the browser stays connected.
/// </remarks>
public sealed class ImportActivity
{
    private readonly Lock gate = new();

    private bool isRunning;

    /// <summary>Raised when <see cref="IsRunning"/> or <see cref="Result"/> changed.</summary>
    /// <remarks>
    /// Raised from whichever thread finished the import, which is not the one
    /// the page renders on — a handler has to marshal before touching the UI.
    /// </remarks>
    public event Action? Changed;

    private readonly StoredDataChanges storedDataChanges;

    /// <summary>Creates the activity of one circuit.</summary>
    /// <param name="storedDataChanges">
    /// Told once an import has run to its end — whatever it said, since even a
    /// refused file was read to the end. Not told about one that threw, one
    /// turned away because another was in flight, or a <see cref="Report"/>.
    /// Narrower than <see cref="Changed"/> on purpose: whoever shows what is
    /// stored has to look again after an import, not every time the Import page
    /// changes what it shows.
    /// </param>
    public ImportActivity(StoredDataChanges storedDataChanges)
    {
        ArgumentNullException.ThrowIfNull(storedDataChanges);

        this.storedDataChanges = storedDataChanges;
    }

    /// <summary>Whether an import is in flight.</summary>
    public bool IsRunning
    {
        get
        {
            lock (gate)
            {
                return isRunning;
            }
        }
    }

    /// <summary>
    /// What the last import said, or <c>null</c> when none has finished in this
    /// session. Kept so that an import which finishes while the page is closed
    /// is not lost.
    /// </summary>
    public ImportFeedbackMessage? Result { get; private set; }

    /// <summary>
    /// Runs <paramref name="import"/> unless one is already running.
    /// </summary>
    /// <param name="import">The work to do. Its message becomes <see cref="Result"/>.</param>
    /// <returns>
    /// <c>false</c> when an import was already in flight and nothing was
    /// started; <c>true</c> once this one has finished.
    /// </returns>
    public async Task<bool> RunAsync(Func<Task<ImportFeedbackMessage>> import)
    {
        ArgumentNullException.ThrowIfNull(import);

        lock (gate)
        {
            if (isRunning)
            {
                return false;
            }

            isRunning = true;
        }

        Result = null;
        Changed?.Invoke();

        try
        {
            Result = await import().ConfigureAwait(false);
        }
        finally
        {
            lock (gate)
            {
                isRunning = false;
            }

            Changed?.Invoke();
        }

        storedDataChanges.Notify();

        return true;
    }

    /// <summary>
    /// Records a message without running anything — for what the page refuses
    /// before it starts, such as a file over the size limit.
    /// </summary>
    /// <param name="message">What to show.</param>
    public void Report(ImportFeedbackMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Result = message;
        Changed?.Invoke();
    }
}
