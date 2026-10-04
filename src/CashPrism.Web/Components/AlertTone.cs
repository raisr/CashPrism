namespace CashPrism.Web.Components;

/// <summary>
/// What an <c>Alert</c> says, and therefore the colour and glyph it is drawn
/// with.
/// </summary>
public enum AlertTone
{
    /// <summary>A note, neither good nor bad.</summary>
    Info = 0,

    /// <summary>Something worked.</summary>
    Success = 1,

    /// <summary>Something worked, but deserves a look.</summary>
    Warning = 2,

    /// <summary>Something was refused or failed.</summary>
    Danger = 3,
}
