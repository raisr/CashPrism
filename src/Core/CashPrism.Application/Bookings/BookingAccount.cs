namespace CashPrism.Application.Bookings;

/// <summary>An account the booking list can be narrowed to.</summary>
/// <param name="Reference">The reference the export identifies the account by — an IBAN or a provider's handle.</param>
/// <param name="Name">The display name the account carries in Finanzguru.</param>
public sealed record BookingAccount(string Reference, string Name);
