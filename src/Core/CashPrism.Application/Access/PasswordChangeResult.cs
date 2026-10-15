namespace CashPrism.Application.Access;

/// <summary>What an attempt to change the password came to.</summary>
/// <param name="Outcome">How it ended.</param>
/// <param name="LockedUntil">
/// When the current password is checked again, or <c>null</c> when it is now —
/// see <see cref="LoginResult.LockedUntil"/>.
/// </param>
public sealed record PasswordChangeResult(PasswordChangeOutcome Outcome, DateTimeOffset? LockedUntil);
