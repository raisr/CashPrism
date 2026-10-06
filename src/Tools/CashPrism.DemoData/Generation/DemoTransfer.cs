namespace CashPrism.DemoData.Generation;

/// <summary>
/// A transfer between two of the household's accounts, as
/// <see cref="DemoPlan.Transfer"/> turns it into two rows.
/// </summary>
public sealed record DemoTransfer
{
    /// <summary>The account the money leaves.</summary>
    public required DemoAccount From { get; init; }

    /// <summary>The account the money arrives on.</summary>
    public required DemoAccount To { get; init; }

    /// <summary>The day of the transfer.</summary>
    public required DateTime Date { get; init; }

    /// <summary>The amount moved, positive.</summary>
    public required long AmountInCents { get; init; }

    /// <summary>How the outgoing row is paid.</summary>
    public required string OutgoingKind { get; init; }

    /// <summary>The outgoing row's category.</summary>
    public required DemoCategory OutgoingCategory { get; init; }

    /// <summary>The incoming row's category.</summary>
    public required DemoCategory IncomingCategory { get; init; }

    /// <summary>How the incoming row is paid; a plain transfer unless the export leaves it blank.</summary>
    public string IncomingKind { get; init; } = DemoTransactionKind.Transfer;

    /// <summary>
    /// Who the outgoing row names as the other party. <see langword="null"/>
    /// names the household itself; a bank that collects the money names itself.
    /// </summary>
    public DemoParty? Payee { get; init; }

    /// <summary>The payment reference, the same on both rows.</summary>
    public string PaymentReference { get; init; } = string.Empty;

    /// <summary>The contract the outgoing row belongs to, or <see langword="null"/>.</summary>
    public DemoContract? Contract { get; init; }

    /// <summary>Whether both rows are left out of the freely disposable income.</summary>
    public bool IsExcludedFromDisposableIncome { get; init; }
}
