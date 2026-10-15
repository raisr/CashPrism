namespace CashPrism.Web.StoredData;

/// <summary>
/// Tells whoever shows what is stored that it has changed and is worth reading
/// again. Registered per circuit, like the activity that raises it.
/// </summary>
/// <remarks>
/// One notice for every way the stored data changes — an import that ran to
/// its end, the data being deleted — so a listener does not have to know each
/// of them. It says that something changed, not what: a listener reads again.
/// Another browser is not told, because each circuit has its own instance.
/// </remarks>
public sealed class StoredDataChanges
{
    /// <summary>Raised when the stored data has changed.</summary>
    /// <remarks>
    /// Raised from whichever thread did the change, which is not necessarily
    /// the one a component renders on — a handler has to marshal before
    /// touching the UI.
    /// </remarks>
    public event Action? Changed;

    /// <summary>Announces that the stored data has changed.</summary>
    public void Notify() => Changed?.Invoke();
}
