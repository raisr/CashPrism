namespace CashPrism.Application.Paging;

/// <summary>
/// The slice of a list something asks for. Every list in this application is
/// read a page at a time, and every page size ultimately arrives from a
/// browser, so the two values that can be wrong are guarded here rather than in
/// each reader.
/// </summary>
/// <param name="Skip">How many entries to pass over. Zero is the first page.</param>
/// <param name="Take">How many entries the page holds.</param>
public record PageRequest(int Skip, int Take)
{
    /// <summary>The largest page anything may ask for.</summary>
    /// <remarks>
    /// On Blazor Server every row a page names is rendered and pushed down the
    /// circuit. The ceiling keeps a tampered-with request from turning a whole
    /// history into one render.
    /// </remarks>
    public const int MaxTake = 500;

    /// <summary>How many entries to pass over. Zero is the first page.</summary>
    public int Skip { get; } = Skip >= 0
        ? Skip
        : throw new ArgumentOutOfRangeException(nameof(Skip), Skip, "A page cannot start before the first entry.");

    /// <summary>How many entries the page holds.</summary>
    public int Take { get; } = Take is > 0 and <= MaxTake
        ? Take
        : throw new ArgumentOutOfRangeException(
            nameof(Take),
            Take,
            $"A page holds between one and {MaxTake} entries.");
}
