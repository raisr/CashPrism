namespace CashPrism.Application.Imports;

/// <summary>How an import ended.</summary>
public enum ImportOutcome
{
    /// <summary>The file was read and its rows were applied.</summary>
    Imported = 0,

    /// <summary>
    /// The same file had been imported before, recognised by its hash, and nothing
    /// was written a second time. Not an error: re-uploading a file is something a
    /// person does, and the honest answer is that there is nothing new in it.
    /// </summary>
    AlreadyImported = 1,

    /// <summary>The file could not be read, and nothing was written.</summary>
    Failed = 2,
}
