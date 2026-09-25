using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the rule from Agents.md that <c>CashPrism.TestSupport</c> is a test
/// project's tool and nothing else: no production project may reference it.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the other rules here this one reads the project files rather than the
/// built assemblies. An unused <c>ProjectReference</c> emits no
/// <c>AssemblyRef</c>, so the metadata approach would pass while
/// <c>CashPrism.TestSupport.dll</c> is copied next to a shipped assembly — and
/// the reference itself is the harm, not the use of it.
/// </para>
/// <para>
/// Production projects are the ones directly under <c>src/</c>; everything
/// below <c>src/Tests/</c> is test-side and may reference it freely.
/// </para>
/// </remarks>
public sealed class TestSupportBoundaryTests
{
    private const string TestSupportProject = "CashPrism.TestSupport";

    public static IEnumerable<object[]> ProductionProjectFiles()
        => ProductionProjects().Select(path => new object[] { path });

    [Theory]
    [MemberData(nameof(ProductionProjectFiles))]
    public void Does_Not_Reference_The_Shared_Test_Fixtures(string projectFile)
    {
        var offenders = ReferencedProjectNames(projectFile)
            .Where(name => string.Equals(name, TestSupportProject, StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Without this the rule above would pass by finding no project at all — a
    /// broken path walk is indistinguishable from a clean reference graph.
    /// </summary>
    [Fact]
    public void Every_Production_Project_Is_Actually_Examined()
    {
        var examined = ProductionProjects()
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();

        Assert.Equal(ProjectAssemblies.All.Order(), examined.Order());
    }

    /// <summary>
    /// Proves the scan can see a reference at all, so an unreadable or
    /// mis-shaped project file cannot make the rule pass silently.
    /// </summary>
    [Fact]
    public void The_Shared_Fixtures_Are_Referenced_By_A_Test_Project()
    {
        var referencing = TestProjects()
            .Where(path => ReferencedProjectNames(path).Contains(TestSupportProject, StringComparer.Ordinal))
            .ToArray();

        Assert.NotEmpty(referencing);
    }

    private static IEnumerable<string> ProductionProjects()
        => Directory.EnumerateFiles(SourceRoot(), "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsUnderTests(path))
            .Order(StringComparer.Ordinal);

    private static IEnumerable<string> TestProjects()
        => Directory.EnumerateFiles(Path.Combine(SourceRoot(), "Tests"), "*.csproj", SearchOption.AllDirectories);

    private static bool IsUnderTests(string projectFile)
    {
        var testsRoot = Path.Combine(SourceRoot(), "Tests") + Path.DirectorySeparatorChar;

        return projectFile.StartsWith(testsRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ReferencedProjectNames(string projectFile)
        => XDocument.Load(projectFile)
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(include => !string.IsNullOrEmpty(include))
            .Select(include => Path.GetFileNameWithoutExtension(include!.Replace('\\', Path.DirectorySeparatorChar)))
            .ToArray();

    /// <summary>
    /// Walks up from the test binary to the <c>src</c> directory that holds the
    /// solution file. The test binary sits several levels deep under
    /// <c>bin/</c>, and its depth is a build detail rather than something to
    /// hard-code.
    /// </summary>
    private static string SourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "CashPrism.slnx");

            if (File.Exists(candidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"No directory containing 'CashPrism.slnx' above '{AppContext.BaseDirectory}'.");
    }
}
