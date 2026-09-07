namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the composition-root rule from Agents.md: only <c>CashPrism.Shell</c>
/// and <c>CashPrism.Anonymiser</c> — the two entry points a process actually
/// starts from — may reference an Infrastructure assembly. Every other project
/// reaches infrastructure concerns only through an interface it declares
/// itself.
/// </summary>
public sealed class InfrastructureBoundaryTests
{
    private static readonly string[] CompositionRoots = ["CashPrism.Shell", "CashPrism.Anonymiser"];

    private static readonly string[] AllProjectAssemblyNames =
    [
        "CashPrism.Domain",
        "CashPrism.Application",
        "CashPrism.Infrastructure",
        "CashPrism.Infrastructure.Finanzguru",
        "CashPrism.Web",
        "CashPrism.Shell",
        "CashPrism.Anonymiser",
    ];

    public static IEnumerable<object[]> NonCompositionRoots()
        => AllProjectAssemblyNames.Except(CompositionRoots).Select(name => new object[] { name });

    [Theory]
    [MemberData(nameof(NonCompositionRoots))]
    public void Does_Not_Reference_An_Infrastructure_Assembly(string assemblyName)
    {
        var offenders = LayeringTests
            .ReferencedAssemblyNames(assemblyName)
            .Where(n => n.StartsWith("CashPrism.Infrastructure", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(offenders);
    }
}
