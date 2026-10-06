using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CashPrism.Web.Tests.Unit;

/// <summary>
/// Pins German for every test in this assembly, as <c>Shell</c> does for the
/// running application, so no test inherits whatever culture the machine
/// running it happens to be set to.
/// </summary>
internal static class GermanCulture
{
    [ModuleInitializer]
    [SuppressMessage(
        "Usage",
        "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
        Justification = "A test assembly is loaded by its test runner only; pinning the culture before the first test is exactly what is intended.")]
    internal static void Pin()
    {
        var culture = new CultureInfo("de-DE");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
