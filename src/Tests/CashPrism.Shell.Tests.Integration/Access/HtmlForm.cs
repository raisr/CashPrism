using System.Net;
using System.Text.RegularExpressions;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Submits a form the way a browser would: the page is fetched first, so the
/// hidden fields it renders — the antiforgery token and the name Blazor gives
/// the form — go back with the fields the test fills in.
/// </summary>
/// <remarks>
/// A browser only sends a field the page rendered with a <c>name</c>, and only
/// sends a ticked checkbox with the value the checkbox carries. Both are
/// checked against the rendered page rather than assumed, so a page whose field
/// lost its name fails here instead of passing with a post no browser would
/// make — on a static page a component library's field renders no name unless
/// it is given one.
/// </remarks>
internal static partial class HtmlForm
{
    /// <summary>
    /// Fetches <paramref name="path"/> and posts its hidden fields, with
    /// <paramref name="fields"/> filled in and <paramref name="ticked"/>
    /// ticked, to <paramref name="action"/> — or back to the page itself, which
    /// is where a Blazor form posts.
    /// </summary>
    /// <param name="client">The client that fetches and posts.</param>
    /// <param name="path">The page the form is on.</param>
    /// <param name="fields">The text fields to fill in, by the name the page renders them with.</param>
    /// <param name="ticked">The checkboxes to tick, by the name the page renders them with.</param>
    /// <param name="action">Where the form posts to, when that is not the page itself.</param>
    public static async Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        string path,
        IReadOnlyDictionary<string, string> fields,
        IReadOnlyCollection<string>? ticked = null,
        string? action = null)
    {
        var html = await client.GetStringAsync(path);

        var inputs = Input().Matches(html)
            .Select(input => Attribute().Matches(input.Value).ToDictionary(
                attribute => attribute.Groups["name"].Value,
                attribute => WebUtility.HtmlDecode(attribute.Groups["value"].Value),
                StringComparer.OrdinalIgnoreCase))
            .Where(attributes => attributes.ContainsKey("name"))
            .ToList();

        var values = inputs
            .Where(attributes => attributes.GetValueOrDefault("type") == "hidden")
            .DistinctBy(attributes => attributes["name"])
            .ToDictionary(attributes => attributes["name"], attributes => attributes.GetValueOrDefault("value") ?? string.Empty);

        foreach (var (name, value) in fields)
        {
            Assert.True(
                inputs.Any(attributes => attributes["name"] == name),
                $"{path} renders no field named {name}, so a browser would not send it.");

            values[name] = value;
        }

        foreach (var name in ticked ?? [])
        {
            var checkbox = inputs.SingleOrDefault(
                attributes => attributes["name"] == name && attributes.GetValueOrDefault("type") == "checkbox");
            Assert.True(checkbox is not null, $"{path} renders no checkbox named {name}, so a browser would not send it.");

            // What a browser sends for a ticked checkbox without a value.
            values[name] = checkbox.GetValueOrDefault("value") ?? "on";
        }

        using var content = new FormUrlEncodedContent(values);

        return await client.PostAsync(action ?? path, content);
    }

    [GeneratedRegex("""<input\b[^>]*>""")]
    private static partial Regex Input();

    [GeneratedRegex(@"(?<name>[\w:.-]+)=""(?<value>[^""]*)""")]
    private static partial Regex Attribute();
}
