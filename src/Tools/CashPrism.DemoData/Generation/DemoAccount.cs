namespace CashPrism.DemoData.Generation;

/// <summary>One account of the demo household, as the export's account columns name it.</summary>
/// <param name="Reference">
/// What <c>Referenzkonto</c> carries: an IBAN for a bank account, a UUID for the
/// credit card and a handle for the payment provider — the three shapes a real
/// export was measured to use.
/// </param>
/// <param name="Name">What <c>Name Referenzkonto</c> carries.</param>
/// <param name="OpeningBalanceInCents">The balance before the first generated booking.</param>
/// <param name="CardLabel">
/// How a card payment's reference names the card, or <see langword="null"/> for
/// an account without a card.
/// </param>
public sealed record DemoAccount(
    string Reference,
    string Name,
    long OpeningBalanceInCents,
    string? CardLabel = null);
