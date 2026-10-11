namespace CashPrism.Application.CashFlow;

/// <summary>What went out to one counterparty, in whole cents.</summary>
/// <param name="Counterparty">The other party, exactly as the export carries it.</param>
/// <param name="SpendingInCents">What went out, as a positive number.</param>
/// <param name="BookingCount">How many bookings added up to it.</param>
public sealed record CounterpartySpending(string Counterparty, long SpendingInCents, int BookingCount);
