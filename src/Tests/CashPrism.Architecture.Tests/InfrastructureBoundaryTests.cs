namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the composition-root rule from Agents.md: only <c>CashPrism.Shell</c>,
/// <c>CashPrism.Anonymiser</c> and <c>CashPrism.DemoData</c> — the three entry
/// points a process actually starts from — may reference an Infrastructure
/// assembly. Every other project
/// reaches infrastructure concerns only through an interface it declares
/// itself.
/// </summary>
public sealed class InfrastructureBoundaryTests
{
    private static readonly string[] CompositionRoots = ["CashPrism.Shell", "CashPrism.Anonymiser", "CashPrism.DemoData"];

    public static IEnumerable<object[]> NonCompositionRoots()
        => ProjectAssemblies.AllExcept(CompositionRoots);

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
