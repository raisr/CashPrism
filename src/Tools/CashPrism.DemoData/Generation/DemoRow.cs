namespace CashPrism.DemoData.Generation;

/// <summary>
/// One row of the demo export, every column worked out. The writer only puts
/// these values into cells; it decides nothing.
/// </summary>
/// <param name="Booking">The booking the row describes.</param>
/// <param name="BalanceInCents">What <c>Kontostand</c> carries.</param>
/// <param name="AmountDirection">What <c>Analyse-Betrag</c> carries.</param>
/// <param name="Week">What <c>Analyse-Woche</c> carries.</param>
/// <param name="Month">What <c>Analyse-Monat</c> carries.</param>
/// <param name="Quarter">What <c>Analyse-Quartal</c> carries.</param>
/// <param name="Year">What <c>Analyse-Jahr</c> carries, a number rather than text.</param>
public sealed record DemoRow(
    DemoBooking Booking,
    long BalanceInCents,
    string AmountDirection,
    string Week,
    string Month,
    string Quarter,
    int Year);
