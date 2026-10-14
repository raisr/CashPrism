using CashPrism.Web.Resources;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace CashPrism.Web.Localisation;

/// <summary>
/// Feeds MudBlazor's own user-visible strings — a dialog's close button, an
/// input's clear button — from <c>Strings.resx</c>, so the component library
/// speaks the same German as everything around it.
/// </summary>
/// <remarks>
/// The keys are MudBlazor's own, written with an underscore:
/// <c>MudDialog_Close</c>, <c>MudInput_Clear</c>. A key the
/// resource file does not carry must come back with
/// <see cref="LocalizedString.ResourceNotFound"/> set — that is the signal
/// MudBlazor falls back to its built-in English default on. Answering for every
/// key instead would render every untranslated label as its raw identifier.
/// <see cref="IStringLocalizer"/> already reports a miss that way, so the
/// lookup is passed straight through.
/// </remarks>
/// <param name="strings">The application's resource set, <c>Strings.resx</c>.</param>
public sealed class ResourceMudLocalizer(IStringLocalizer<Strings> strings) : MudLocalizer
{
    /// <inheritdoc />
    public override LocalizedString this[string key] => strings[key];
}
