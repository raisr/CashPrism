using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using CashPrism.Application.Bookings;

namespace CashPrism.Web.Bookings;

/// <summary>
/// The state of the booking list as it travels in the URL: the filter, the
/// order and the page. The URL is where it lives, so a reload keeps it, the
/// browser's back button walks through it and another page can link to a
/// filtered list.
/// </summary>
/// <remarks>
/// Everything here arrives from a browser and may have been typed by hand, so
/// <see cref="Parse"/> never throws: a value it cannot read falls back to the
/// list's default, the way an unknown parameter is ignored.
/// </remarks>
/// <param name="Filter">Which bookings are listed.</param>
/// <param name="SortBy">The column a person sorted by, or <see langword="null"/> for the order the list opens in.</param>
/// <param name="Descending">Whether the sorted column runs from the largest value down.</param>
/// <param name="PageIndex">The page shown, zero for the first.</param>
/// <param name="PageSize">How many bookings a page holds, one of <see cref="PageSizes"/>.</param>
public sealed record BookingListQuery(
    BookingFilter Filter,
    BookingSortColumn? SortBy,
    bool Descending,
    int PageIndex,
    int PageSize)
{
    /// <summary>The page sizes the list offers.</summary>
    public static IReadOnlyList<int> PageSizes { get; } = [25, 50, 100];

    /// <summary>The list as it opens: every booking, newest first, the first page.</summary>
    public static BookingListQuery Default { get; } = new(BookingFilter.None, null, true, 0, PageSizes[0]);

    // A page number beyond this is no page anyone paged to, and multiplied by
    // the largest page size it still fits the skip into an int.
    private const int LastReadablePage = 1_000_000;

    private const string DateFormat = "yyyy-MM-dd";

    private static readonly Dictionary<string, BookingSortColumn> SortColumns = new(StringComparer.Ordinal)
    {
        ["counterparty"] = BookingSortColumn.Counterparty,
        ["date"] = BookingSortColumn.BookedOn,
        ["amount"] = BookingSortColumn.AmountInCents,
    };

    private static readonly Dictionary<string, BookingDirection> Directions = new(StringComparer.Ordinal)
    {
        ["income"] = BookingDirection.Income,
        ["spending"] = BookingDirection.Spending,
    };

    /// <summary>
    /// Reads the state out of a URL's query, with or without its leading
    /// <c>?</c>. What is missing or cannot be read is left at its default.
    /// </summary>
    /// <param name="query">The query part of the URL.</param>
    public static BookingListQuery Parse(string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var values = HttpUtility.ParseQueryString(query);

        var category = Text(values, "category");
        var sortBy = SortColumns.TryGetValue(values["sort"] ?? string.Empty, out var column) ? column : (BookingSortColumn?)null;

        var filter = new BookingFilter(
            AccountReference: Text(values, "account"),
            Category: category,
            SubCategory: category is null ? null : Text(values, "subcategory"),
            Period: Period(values),
            Counterparty: Text(values, "counterparty"),
            Direction: Directions.TryGetValue(values["direction"] ?? string.Empty, out var direction) ? direction : null,
            HideTransfers: values["transfers"] == "hide",
            Search: BookingSearch.Parse(values["q"]));

        var page = int.TryParse(values["page"], NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            && number is >= 1 and <= LastReadablePage
                ? number - 1
                : 0;

        var size = int.TryParse(values["size"], NumberStyles.None, CultureInfo.InvariantCulture, out var asked)
            && PageSizes.Contains(asked)
                ? asked
                : Default.PageSize;

        return new BookingListQuery(
            filter,
            sortBy,
            sortBy is null || values["order"] != "asc",
            page,
            size);
    }

    /// <summary>
    /// The query parameters that carry this state. Every parameter the list
    /// knows is named, and the ones at their default are <see langword="null"/>,
    /// which is how a <c>NavigationManager</c> leaves a parameter out of a URL.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ToParameters()
    {
        var sort = SortBy is { } column ? SortColumns.Single(pair => pair.Value == column).Key : null;

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["account"] = Filter.AccountReference,
            ["category"] = Filter.Category,
            ["subcategory"] = Filter.SubCategory,
            ["from"] = Filter.Period?.From.ToString(DateFormat, CultureInfo.InvariantCulture),
            ["to"] = Filter.Period?.To.ToString(DateFormat, CultureInfo.InvariantCulture),
            ["counterparty"] = Filter.Counterparty,
            ["direction"] = Filter.Direction is { } direction ? Directions.Single(pair => pair.Value == direction).Key : null,
            ["transfers"] = Filter.HideTransfers ? "hide" : null,
            ["q"] = Filter.Search?.Text,
            ["sort"] = sort,
            ["order"] = sort is not null && !Descending ? "asc" : null,
            ["page"] = PageIndex > 0 ? (PageIndex + 1).ToString(CultureInfo.InvariantCulture) : null,
            ["size"] = PageSize != Default.PageSize ? PageSize.ToString(CultureInfo.InvariantCulture) : null,
        };
    }

    /// <summary>
    /// A link from another page to the booking list narrowed to
    /// <paramref name="filter"/>, in the order and at the page size the list
    /// opens with. Relative, so it resolves against the app's base address
    /// wherever CashPrism is reached from.
    /// </summary>
    /// <param name="filter">Which bookings the list opens on.</param>
    public static string LinkFor(BookingFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = (Default with { Filter = filter }).ToParameters()
            .Where(parameter => parameter.Value is not null)
            .Select(parameter => parameter.Key + "=" + Uri.EscapeDataString((string)parameter.Value!));

        return "bookings?" + string.Join('&', query);
    }

    /// <summary>The page this state asks the reader for.</summary>
    public BookingPageRequest ToPageRequest()
        => new(
            Skip: PageIndex * PageSize,
            Take: PageSize,
            SortBy: SortBy ?? BookingSortColumn.BookedOn,
            Descending: SortBy is null || Descending,
            Filter: Filter);

    private static string? Text(NameValueCollection values, string name)
        => values[name] is { Length: > 0 } text ? text : null;

    // Both ends or none: half a period says nothing about which bookings are
    // meant, and a reversed one is no period at all.
    private static BookingPeriod? Period(NameValueCollection values)
        => Date(values["from"]) is { } from && Date(values["to"]) is { } to && from <= to
            ? new BookingPeriod(from, to)
            : null;

    private static DateOnly? Date(string? text)
        => DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
}
