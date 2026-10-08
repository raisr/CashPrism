using System.Net;
using System.Text.RegularExpressions;

namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Submits a form the way a browser would: the page is fetched first, so the
/// hidden fields it renders — the antiforgery token and the name Blazor gives
/// the form — go back with the fields the test fills in.
/// </summary>
internal static partial class HtmlForm
{
    /// <summary>
    /// Fetches <paramref name="path"/> and posts its hidden fields, with
    /// <paramref name="fields"/> filled in, to <paramref name="action"/> — or
    /// back to the page itself, which is where a Blazor form posts.
    /// </summary>
    public static async Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        string path,
        IReadOnlyDictionary<string, string> fields,
        string? action = null)
    {
        var html = await client.GetStringAsync(path);

        var values = HiddenInput().Matches(html)
            .DistinctBy(match => match.Groups["name"].Value)
            .ToDictionary(
                match => WebUtility.HtmlDecode(match.Groups["name"].Value),
                match => WebUtility.HtmlDecode(match.Groups["value"].Value));

        foreach (var (name, value) in fields)
        {
            values[name] = value;
        }

        using var content = new FormUrlEncodedContent(values);

        return await client.PostAsync(action ?? path, content);
    }

    [GeneratedRegex("""<input type="hidden" name="(?<name>[^"]*)" value="(?<value>[^"]*)"\s*/?>""")]
    private static partial Regex HiddenInput();
}
