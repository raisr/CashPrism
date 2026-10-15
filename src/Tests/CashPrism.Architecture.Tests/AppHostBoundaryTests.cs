using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the rule from Agents.md that <c>CashPrism.AppHost</c> starts the
/// container image and nothing else: it references no project, so it can
/// never run <c>CashPrism.Shell</c>'s build output in place of the image.
/// </summary>
/// <remarks>
/// Reads the project file rather than the built assembly, for the reason
/// <see cref="TestSupportBoundaryTests"/> gives: an Aspire AppHost does not
/// even reference the output of a project it lists, so the metadata would show
/// nothing either way.
/// </remarks>
public sealed class AppHostBoundaryTests
{
    private const string AppHostProject = "CashPrism.AppHost";

    [Fact]
    public void References_No_Project()
    {
        var projectFile = SourceTree.ProductionProjects()
            .Single(path => Path.GetFileNameWithoutExtension(path) == AppHostProject);

        var references = XDocument.Load(projectFile)
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .ToArray();

        Assert.Empty(references);
    }
}
