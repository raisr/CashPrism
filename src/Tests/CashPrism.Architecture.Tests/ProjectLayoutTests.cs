using System.IO;
using System.Linq;

namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the rule from Agents.md on where a production project lives: the
/// application under <c>src/Core/</c>, a development tool under
/// <c>src/Tools/</c>, and <c>CashPrism.Shell</c>, which only starts the
/// application, at the root of <c>src/</c>.
/// </summary>
public sealed class ProjectLayoutTests
{
    private const string ShellProject = "CashPrism.Shell";

    private static readonly string[] Homes = ["Core", "Tools"];

    public static IEnumerable<object[]> ProductionProjectFiles()
        => SourceTree.ProductionProjects().Select(path => new object[] { path });

    [Theory]
    [MemberData(nameof(ProductionProjectFiles))]
    public void Lies_Under_Core_Or_Tools_Unless_It_Is_The_Shell(string projectFile)
    {
        var directory = Path.GetRelativePath(SourceTree.Root(), Path.GetDirectoryName(projectFile)!);
        var home = directory.Split(Path.DirectorySeparatorChar)[0];

        Assert.True(
            Homes.Contains(home, StringComparer.Ordinal) || directory == ShellProject,
            $"'{directory}' is neither under {string.Join(" nor ", Homes)} nor the shell.");
    }
}
