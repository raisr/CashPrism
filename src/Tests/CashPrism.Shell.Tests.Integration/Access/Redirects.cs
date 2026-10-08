namespace CashPrism.Shell.Tests.Integration.Access;

/// <summary>
/// Where a response sends the browser, as path and query. A redirect from a
/// Blazor page names the whole address and one from the cookie handler may
/// not; both mean the same page.
/// </summary>
internal static class Redirects
{
    public static string? Target(this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.Headers.Location is not { } location
            ? null
            : location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
    }

    public static string? TargetPath(this HttpResponseMessage response)
        => response.Target()?.Split('?')[0];
}
