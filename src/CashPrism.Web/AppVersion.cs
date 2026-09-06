using System.Reflection;

namespace CashPrism.Web;

/// <summary>
/// The product version displayed in the UI, taken from the informational version
/// of this assembly. All projects share the number set in
/// <c>src/Directory.Build.props</c>.
/// </summary>
public static class AppVersion
{
    /// <summary>
    /// The version without the build metadata the SDK appends, e.g. <c>0.1.0</c>.
    /// </summary>
    public static string Current { get; } = Read();

    private static string Read()
    {
        var informational = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informational))
        {
            return "0.0.0";
        }

        // The SDK appends "+<source revision>" when the build knows the commit.
        var metadata = informational.IndexOf('+');

        return metadata < 0 ? informational : informational[..metadata];
    }
}
