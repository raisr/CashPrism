namespace CashPrism.Architecture.Tests;

/// <summary>
/// The assemblies the solution's own projects build into. Several rules here
/// read "every project except …", so the list is kept in one place rather than
/// restated per rule.
/// </summary>
internal static class ProjectAssemblies
{
    /// <summary>Every project assembly, in dependency order.</summary>
    internal static readonly string[] All =
    [
        "CashPrism.Domain",
        "CashPrism.Application",
        "CashPrism.Infrastructure",
        "CashPrism.Infrastructure.Finanzguru",
        "CashPrism.Web",
        "CashPrism.Shell",
        "CashPrism.Anonymiser",
    ];

    /// <summary>
    /// Every project assembly but the named ones, shaped for <c>MemberData</c>.
    /// </summary>
    internal static IEnumerable<object[]> AllExcept(params string[] excluded)
        => All.Except(excluded).Select(name => new object[] { name });
}
