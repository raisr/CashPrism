using System.Globalization;

namespace CashPrism.Web.Localisation;

/// <summary>
/// The line a data grid's pager shows: which entries of how many are on screen.
/// </summary>
/// <remarks>
/// It is written here rather than left to MudBlazor because MudBlazor formats
/// the three counts with a culture of its own and hands its format string
/// finished text — which put an English thousands separator (<c>6,327</c>) into
/// a German page, next to amounts set with a German one. A page therefore
/// composes the sentence itself and passes the pager a finished line, with no
/// placeholder left in it for MudBlazor to fill.
/// </remarks>
public static class PagerInfo
{
    /// <summary>
    /// Writes the info line.
    /// </summary>
    /// <param name="format">
    /// The localised format, with three numeric placeholders — the
    /// <c>MudDataGridPager_InfoFormat</c> entry of <c>Strings.resx</c>.
    /// </param>
    /// <param name="first">The first entry on the page, counted from one. Zero when the page is empty.</param>
    /// <param name="last">The last entry on the page, counted from one. Zero when the page is empty.</param>
    /// <param name="total">How many entries there are in total.</param>
    /// <param name="formatProvider">
    /// The culture to format in. Defaults to the current one, which the host
    /// pins to German — the UI never reads a culture of its own.
    /// </param>
    public static string Compose(
        string format,
        int first,
        int last,
        int total,
        IFormatProvider? formatProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        return string.Format(formatProvider ?? CultureInfo.CurrentCulture, format, first, last, total);
    }

    /// <summary>
    /// The number of the first entry of a page, counted from one, or zero when
    /// the page carries nothing.
    /// </summary>
    /// <param name="skip">How many entries the page passed over.</param>
    /// <param name="onPage">How many entries the page carries.</param>
    public static int First(int skip, int onPage) => onPage == 0 ? 0 : skip + 1;

    /// <summary>
    /// The number of the last entry of a page, counted from one, or zero when
    /// the page carries nothing.
    /// </summary>
    /// <param name="skip">How many entries the page passed over.</param>
    /// <param name="onPage">How many entries the page carries.</param>
    public static int Last(int skip, int onPage) => onPage == 0 ? 0 : skip + onPage;
}
