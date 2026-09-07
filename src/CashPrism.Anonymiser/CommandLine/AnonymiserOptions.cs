namespace CashPrism.Anonymiser.CommandLine;

/// <summary>
/// The parsed command-line arguments of one run: which files to read, where to
/// write the anonymised copies, and whether an existing output file may be
/// overwritten.
/// </summary>
/// <param name="InputFiles">
/// The <c>.xlsx</c> files to process, in the order they were given. At least one.
/// </param>
/// <param name="OutputDirectory">
/// The directory the anonymised files are written into. Mandatory and has no
/// default — an anonymised file must never land next to the real one by
/// accident.
/// </param>
/// <param name="Force">
/// Whether an existing output file may be overwritten. Defaults to
/// <see langword="false"/>: a tool of this kind must not overwrite unasked.
/// </param>
public sealed record AnonymiserOptions(
    IReadOnlyList<string> InputFiles,
    string OutputDirectory,
    bool Force);
