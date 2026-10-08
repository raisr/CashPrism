namespace CashPrism.Application.Access;

/// <summary>How an attempt to change the password ended.</summary>
public enum PasswordChangeOutcome
{
    /// <summary>The new password is set; every login issued before it no longer counts.</summary>
    Done = 0,

    /// <summary>The current password was wrong.</summary>
    Rejected = 1,

    /// <summary>Too many wrong passwords; the attempt was refused without being checked.</summary>
    LockedOut = 2,

    /// <summary>The new password is shorter than the rule allows.</summary>
    TooShort = 3,
}
