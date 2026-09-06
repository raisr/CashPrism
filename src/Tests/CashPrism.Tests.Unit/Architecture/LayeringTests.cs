using System.Linq;
using System.Reflection;

namespace CashPrism.Tests.Unit.Architecture;

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

    private static IEnumerable<string> ReferencedAssemblyNames(string assemblyName)
        => Assembly.Load(assemblyName)
            .GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.Length > 0);

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
