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

    /// <summary>
    /// How many pages a list of <paramref name="total"/> entries fills — at
    /// least one, so an empty list still has a page to be on.
    /// </summary>
    /// <param name="total">How many entries there are in total.</param>
    /// <param name="pageSize">How many entries a page holds.</param>
    public static int PageCount(int total, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        return Math.Max(1, (total + pageSize - 1) / pageSize);
    }

    /// <summary>
    /// The page numbers a pager offers, as the design draws them: the two pages
    /// on either side of the current one, the first and the last page always,
    /// and a gap wherever pages are left out between them.
    /// </summary>
    /// <param name="page">The current page, counted from zero.</param>
    /// <param name="pageCount">How many pages there are.</param>
    /// <returns>
    /// The pages counted from zero, in order, with <c>null</c> standing for a
    /// gap of one or more pages.
    /// </returns>
    public static IReadOnlyList<int?> Pages(int page, int pageCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageCount);
        ArgumentOutOfRangeException.ThrowIfNegative(page);

        var from = Math.Max(0, page - 2);
        var to = Math.Min(pageCount - 1, page + 2);
        var pages = new List<int?>();

        if (from > 0)
        {
            pages.Add(0);
        }

        if (from > 1)
        {
            pages.Add(null);
        }

        for (var near = from; near <= to; near++)
        {
            pages.Add(near);
        }

        if (to < pageCount - 2)
        {
            pages.Add(null);
        }

        if (to < pageCount - 1)
        {
            pages.Add(pageCount - 1);
        }

        return pages;
    }
}
