namespace CashPrism.Application.Access;

/// <summary>What a login attempt came to.</summary>
/// <param name="Outcome">How it ended.</param>
/// <param name="LockedUntil">
/// When logins are accepted again, or <c>null</c> when they are now. Set on a
/// <see cref="LoginOutcome.Rejected"/> as well, when that wrong password was the
/// one that started a lockout.
/// </param>
public sealed record LoginResult(LoginOutcome Outcome, DateTimeOffset? LockedUntil);
