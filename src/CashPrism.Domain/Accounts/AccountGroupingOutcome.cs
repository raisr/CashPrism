namespace CashPrism.Domain.Accounts;

/// <summary>How a change to the account groups ended.</summary>
public enum AccountGroupingOutcome
{
    /// <summary>The change is made — or there was nothing to change, such as moving the topmost group up.</summary>
    Done = 0,

    /// <summary>The name is empty or only white space.</summary>
    NameMissing = 1,

    /// <summary>The group still holds an account that is shown, so it cannot be deleted.</summary>
    GroupNotEmpty = 2,

    /// <summary>The group is the only one, so it cannot be deleted.</summary>
    LastGroup = 3,

    /// <summary>No group has that identity — deleted in another browser, typically.</summary>
    GroupNotFound = 4,

    /// <summary>No shown account has that reference.</summary>
    AccountNotFound = 5,
}
