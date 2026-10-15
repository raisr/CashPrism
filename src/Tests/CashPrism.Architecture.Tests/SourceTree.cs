using System.IO;
using System.Linq;

namespace CashPrism.Architecture.Tests;

/// <summary>
/// The project files under <c>src/</c>, for the rules that read them rather
/// than the built assemblies. Everything below <c>src/Tests/</c> is test-side;
/// every other project is a production project.
/// </summary>
internal static class SourceTree
{
    /// <summary>Every production project file, in ordinal order.</summary>
    internal static IEnumerable<string> ProductionProjects()
        => Directory.EnumerateFiles(Root(), "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsUnderTests(path))
            .Order(StringComparer.Ordinal);

    /// <summary>Every project file below <c>src/Tests/</c>.</summary>
    internal static IEnumerable<string> TestProjects()
        => Directory.EnumerateFiles(Path.Combine(Root(), "Tests"), "*.csproj", SearchOption.AllDirectories);

    /// <summary>
    /// Walks up from the test binary to the <c>src</c> directory that holds the
    /// solution file. The test binary sits several levels deep under
    /// <c>bin/</c>, and its depth is a build detail rather than something to
    /// hard-code.
    /// </summary>
    internal static string Root()
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

    private static bool IsUnderTests(string projectFile)
    {
        var testsRoot = Path.Combine(Root(), "Tests") + Path.DirectorySeparatorChar;

        return projectFile.StartsWith(testsRoot, StringComparison.OrdinalIgnoreCase);
    }
}
