namespace CashPrism.Architecture.Tests;

/// <summary>
/// The assemblies the solution's <b>production</b> projects build into. Several
/// rules here read "every project except …", so the list is kept in one place
/// rather than restated per rule.
/// </summary>
/// <remarks>
/// Test projects are deliberately absent, and their absence is the rule rather
/// than an omission: a test may reference whatever it has to in order to assert
/// something, an Infrastructure project included, because it wires nothing for a
/// use case to run on. See <c>Agents.md</c>.
/// </remarks>
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
        "CashPrism.DemoData",
    ];

    /// <summary>
    /// Every project assembly but the named ones, shaped for <c>MemberData</c>.
    /// </summary>
    internal static IEnumerable<object[]> AllExcept(params string[] excluded)
        => All.Except(excluded).Select(name => new object[] { name });
}
