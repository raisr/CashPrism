namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the rule from Agents.md that the UI is a layer like any other: how a
/// page is drawn is a detail of <c>CashPrism.Web</c>. Only that project may
/// reference the component library — not the host that starts the process, and
/// certainly not anything inside the onion.
/// </summary>
public sealed class ComponentLibraryBoundaryTests
{
    private const string ComponentLibrary = "MudBlazor";

    private const string PresentationAssembly = "CashPrism.Web";

    public static IEnumerable<object[]> EveryProjectButThePresentationLayer()
        => ProjectAssemblies.AllExcept(PresentationAssembly);

    [Theory]
    [MemberData(nameof(EveryProjectButThePresentationLayer))]
    public void Does_Not_Reference_The_Component_Library(string assemblyName)
    {
        var offenders = LayeringTests
            .ReferencedAssemblyNames(assemblyName)
            .Where(n => string.Equals(n, ComponentLibrary, StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Without this the rule above would keep passing after the component
    /// library was renamed or dropped, asserting nothing at all.
    /// </summary>
    [Fact]
    public void Presentation_Layer_Does_Reference_The_Component_Library()
    {
        var references = LayeringTests.ReferencedAssemblyNames(PresentationAssembly);

        Assert.Contains(ComponentLibrary, references);
    }
}
