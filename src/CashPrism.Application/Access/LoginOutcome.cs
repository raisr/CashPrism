namespace CashPrism.Application.Access;

/// <summary>How a login attempt ended.</summary>
public enum LoginOutcome
{
    /// <summary>The password was right.</summary>
    Accepted = 0,

    /// <summary>The password was wrong, or no password is set yet.</summary>
    Rejected = 1,

    /// <summary>Too many wrong passwords; the attempt was refused without being checked.</summary>
    LockedOut = 2,
}
