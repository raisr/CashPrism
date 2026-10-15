namespace CashPrism.Web.Imports;

/// <summary>
/// How an upload ended, in the terms the page shows it — deliberately not
/// MudBlazor's <c>Severity</c>, so that what the page says stays testable
/// without a component library.
/// </summary>
public enum ImportFeedbackSeverity
{
    /// <summary>The file was imported.</summary>
    Success = 0,

    /// <summary>Nothing went wrong and nothing happened either.</summary>
    Info = 1,

    /// <summary>The file was not imported.</summary>
    Error = 2,
}
