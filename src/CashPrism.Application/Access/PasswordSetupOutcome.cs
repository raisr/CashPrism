namespace CashPrism.Application.Access;

/// <summary>How an attempt to set the first password ended.</summary>
public enum PasswordSetupOutcome
{
    /// <summary>The password is set.</summary>
    Done = 0,

    /// <summary>A password was already set; setting it again is not what this does.</summary>
    AlreadySet = 1,

    /// <summary>The setup code was not the one this start printed.</summary>
    WrongCode = 2,

    /// <summary>The password is shorter than the rule allows.</summary>
    TooShort = 3,
}
