namespace CashPrism.Web.Access;

/// <summary>
/// Where to go after logging in. The login page is sent the address that was
/// asked for, and following it blindly would turn the login into a redirect to
/// any site someone puts into a link.
/// </summary>
public static class ReturnUrl
{
    /// <summary>The start page, where a login goes when it was sent nowhere usable.</summary>
    public const string Fallback = "/";

    /// <summary>
    /// <paramref name="returnUrl"/> if it is a path within this application,
    /// otherwise <see cref="Fallback"/>.
    /// </summary>
    /// <param name="returnUrl">The address the login page was sent, or <c>null</c>.</param>
    public static string Resolve(string? returnUrl)
    {
        // A local path starts with exactly one slash. "//host" and "/\host" are
        // read by browsers as an address on another host.
        var isLocal = returnUrl is { Length: > 0 }
            && returnUrl[0] == '/'
            && (returnUrl.Length == 1 || returnUrl[1] is not ('/' or '\\'));

        return isLocal ? returnUrl! : Fallback;
    }
}
