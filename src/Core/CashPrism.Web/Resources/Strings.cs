namespace CashPrism.Web.Resources;

/// <summary>
/// The anchor type for the UI's string resources: <c>IStringLocalizer&lt;Strings&gt;</c>
/// resolves against <c>Strings.resx</c> next to this file. It exists only to give
/// the resource set a name a component can inject — there is nothing to
/// instantiate.
/// </summary>
/// <remarks>
/// The resource file is neutral German: English keys, German values, and no
/// culture-specific sibling. A second language is a new
/// <c>Strings.&lt;culture&gt;.resx</c> and changes nothing here. Why the UI is
/// German at all is in <c>Agents.md</c>.
/// </remarks>
public sealed class Strings
{
    private Strings()
    {
    }
}
