namespace CashPrism.Application.Access;

/// <summary>What a login attempt came to.</summary>
/// <param name="Outcome">How it ended.</param>
/// <param name="LockedUntil">
/// When logins are accepted again, or <c>null</c> when they are now. Set on a
/// <see cref="LoginOutcome.Rejected"/> as well, when that wrong password was the
/// one that started a lockout.
/// </param>
/// <param name="Generation">
/// On <see cref="LoginOutcome.Accepted"/>, the generation of the password that
/// was right, for the login to carry; <c>null</c> otherwise.
/// </param>
public sealed record LoginResult(LoginOutcome Outcome, DateTimeOffset? LockedUntil, int? Generation = null);
