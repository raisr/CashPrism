namespace CashPrism.Web.Imports;

/// <summary>
/// One message for the person who just uploaded a file.
/// </summary>
/// <param name="Severity">How it ended.</param>
/// <param name="Headline">The one line that is always shown.</param>
/// <param name="Details">
/// The lines under it: the four counts of a successful import, or every reason
/// a file was refused. Empty where the headline says everything.
/// </param>
public sealed record ImportFeedbackMessage(
    ImportFeedbackSeverity Severity,
    string Headline,
    IReadOnlyList<string> Details);
