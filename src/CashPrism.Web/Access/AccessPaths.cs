namespace CashPrism.Web.Access;

/// <summary>
/// The addresses that let a person in and out. Named once, because the cookie
/// options, the pages and the logout form all have to agree on them.
/// </summary>
public static class AccessPaths
{
    /// <summary>The login page.</summary>
    public const string Login = "/login";

    /// <summary>The page that sets the first password.</summary>
    public const string Setup = "/setup";

    /// <summary>Where the logout form posts to.</summary>
    public const string Logout = "/logout";

    /// <summary>The login page, telling the person that the password was just set.</summary>
    public const string LoginAfterSetup = Login + "?setup=done";
}
