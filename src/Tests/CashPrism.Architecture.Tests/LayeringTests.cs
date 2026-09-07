using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace CashPrism.Architecture.Tests;

/// <summary>
/// Guards the onion dependency rule from Agents.md: the inner layers must not
/// take a dependency on infrastructure concerns. These tests have real teeth
/// once the layers hold code; today they lock the direction in from the start.
/// </summary>
public sealed class LayeringTests
{
    private static readonly string[] ForbiddenInnerDependencies =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "ClosedXML",
    ];

    /// <summary>
    /// Reads the <c>AssemblyRef</c> table straight from the project DLL's bytes
    /// via <see cref="PEReader"/>/<see cref="MetadataReader"/>. Deliberately not
    /// <c>Assembly.Load</c>: that loads the DLL as executable code just to read
    /// its metadata, which an application-control policy (WDAC / Smart App
    /// Control) can deny for a freshly built, unsigned local DLL.
    /// </summary>
    internal static IEnumerable<string> ReferencedAssemblyNames(string assemblyName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.dll");
        using var stream = File.OpenRead(path);
        using var peReader = new PEReader(stream);
        var metadataReader = peReader.GetMetadataReader();

        return metadataReader.AssemblyReferences
            .Select(handle => metadataReader.GetString(metadataReader.GetAssemblyReference(handle).Name))
            .Where(n => n.Length > 0)
            .ToArray();
    }

    public sealed class Domain
    {
        [Fact]
        public void Depends_On_Nothing_Outside_The_Bcl()
        {
            var offenders = ReferencedAssemblyNames("CashPrism.Domain")
                .Where(n => ForbiddenInnerDependencies.Any(n.StartsWith))
                .ToArray();

            Assert.Empty(offenders);
        }
    }

    public sealed class Application
    {
        [Fact]
        public void Does_Not_Depend_On_Infrastructure_Concerns()
        {
            var offenders = ReferencedAssemblyNames("CashPrism.Application")
                .Where(n => ForbiddenInnerDependencies.Any(n.StartsWith))
                .ToArray();

            Assert.Empty(offenders);
        }
    }
}
