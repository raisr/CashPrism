namespace CashPrism.Domain.Accounts;

/// <summary>
/// The balance of one account as one export reports it. It is a projection of
/// that export, like a booking: a later export brings a later balance, and
/// nothing about it is CashPrism's own.
/// </summary>
/// <remarks>
/// It is taken once, while the export is imported, because the rule that finds
/// it needs the order of the file — see <see cref="AccountBalanceRule"/> — and
/// the stored bookings do not keep that order.
/// </remarks>
public sealed class AccountBalance
{
    /// <summary>
    /// Creates a balance.
    /// </summary>
    /// <param name="importRunId">The run whose export reported the balance.</param>
    /// <param name="accountReference">
    /// The account — an IBAN, or the handle a provider account is identified by.
    /// </param>
    /// <param name="accountName">The display name the account carries in Finanzguru.</param>
    /// <param name="balanceInCents">The balance in whole cents.</param>
    /// <param name="currency">The ISO 4217 code the balance is in.</param>
    /// <param name="latestBookedOn">The date of the account's newest booking in that export.</param>
    /// <exception cref="ArgumentException">A required value is missing.</exception>
    public AccountBalance(
        Guid importRunId,
        string accountReference,
        string accountName,
        long balanceInCents,
        string currency,
        DateTime latestBookedOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountReference);
        ArgumentNullException.ThrowIfNull(accountName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (importRunId == Guid.Empty)
        {
            throw new ArgumentException("A balance needs the import run that reported it.", nameof(importRunId));
        }

        ImportRunId = importRunId;
        AccountReference = accountReference;
        AccountName = accountName;
        BalanceInCents = balanceInCents;
        Currency = currency;
        LatestBookedOn = latestBookedOn;
    }

    /// <summary>The run whose export reported the balance.</summary>
    public Guid ImportRunId { get; }

    /// <summary>The account the balance belongs to.</summary>
    public string AccountReference { get; }

    /// <summary>The display name the account carries in Finanzguru.</summary>
    public string AccountName { get; }

    /// <summary>The balance in whole cents.</summary>
    public long BalanceInCents { get; }

    /// <summary>The ISO 4217 code the balance is in.</summary>
    public string Currency { get; }

    /// <summary>The date of the account's newest booking in the export.</summary>
    public DateTime LatestBookedOn { get; }
}
