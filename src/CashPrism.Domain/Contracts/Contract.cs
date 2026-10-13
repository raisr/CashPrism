namespace CashPrism.Domain.Contracts;

/// <summary>
/// A recurring booking Finanzguru recognised as a contract, as its newest
/// booking describes it, and the rules derived from it.
/// </summary>
/// <remarks>
/// <para>
/// The export carries a contract only through its bookings: a flag, an
/// interval and an identifier on each. Name, sub-category and amount are those
/// of the newest booking, because a contract's amount and counterparty name do
/// vary over time and the newest booking is what it costs now. Within one
/// contract the interval never varies and the sign never mixes — measured, see
/// <c>docs/finanzguru-export.md</c>.
/// </para>
/// <para>
/// Nothing here is CashPrism's own state. A contract removed in the app loses
/// its columns in the next export, and one marked as cancelled there keeps
/// them unchanged, so whether a contract has ended can only be derived — see
/// <see cref="IsPresumablyEnded"/>.
/// </para>
/// </remarks>
public sealed class Contract
{
    /// <summary>
    /// How many intervals may pass without a booking before a contract counts
    /// as presumably ended.
    /// </summary>
    /// <remarks>
    /// One interval is not enough: a payment is often booked a few days late,
    /// and an export taken just before the due date would declare every such
    /// contract ended. Two intervals means at least one payment has been missed
    /// outright, which a running contract does not do.
    /// </remarks>
    public const int EndedAfterIntervals = 2;

    /// <summary>
    /// How many days a booking may come after its expected date before the
    /// contract counts as overdue, the same for every interval.
    /// </summary>
    /// <remarks>
    /// Measured on the maintainer's export: of the contract bookings that came
    /// later than expected, nearly all came one to four days late, and the few
    /// beyond five were payments missed outright.
    /// </remarks>
    public const int OverdueToleranceInDays = 5;

    private static readonly Dictionary<string, int> MonthsPerInterval = new(StringComparer.Ordinal)
    {
        ["monatlich"] = 1,
        ["zweimonatlich"] = 2,
        ["vierteljaehrlich"] = 3,
        ["halbjaehrlich"] = 6,
        ["jaehrlich"] = 12,
    };

    /// <summary>
    /// Creates a contract from its newest booking.
    /// </summary>
    /// <param name="contractId">Finanzguru's identifier of the contract.</param>
    /// <param name="interval">How often the contract recurs, as Finanzguru names it.</param>
    /// <param name="counterparty">The other party of the newest booking.</param>
    /// <param name="category">Finanzguru's main category of the newest booking.</param>
    /// <param name="subCategory">Finanzguru's sub-category of the newest booking.</param>
    /// <param name="amountInCents">The signed amount of the newest booking, in whole cents.</param>
    /// <param name="currency">The ISO 4217 code the amount is in.</param>
    /// <param name="newestBookedOn">The date of the newest booking.</param>
    /// <param name="isTransfer">Whether the newest booking is a transfer between two of the owner's own accounts.</param>
    /// <exception cref="ArgumentException">A required value is missing.</exception>
    public Contract(
        string contractId,
        string interval,
        string counterparty,
        string category,
        string subCategory,
        long amountInCents,
        string currency,
        DateTime newestBookedOn,
        bool isTransfer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractId);
        ArgumentNullException.ThrowIfNull(interval);
        ArgumentNullException.ThrowIfNull(counterparty);
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(subCategory);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        ContractId = contractId;
        Interval = interval;
        Counterparty = counterparty;
        Category = category;
        SubCategory = subCategory;
        AmountInCents = amountInCents;
        Currency = currency;
        NewestBookedOn = newestBookedOn;
        IsTransfer = isTransfer;
    }

    /// <summary>Finanzguru's identifier of the contract.</summary>
    public string ContractId { get; }

    /// <summary>How often the contract recurs, as Finanzguru names it.</summary>
    public string Interval { get; }

    /// <summary>The other party of the newest booking.</summary>
    public string Counterparty { get; }

    /// <summary>Finanzguru's main category of the newest booking.</summary>
    public string Category { get; }

    /// <summary>Finanzguru's sub-category of the newest booking.</summary>
    public string SubCategory { get; }

    /// <summary>The signed amount of the newest booking, in whole cents.</summary>
    public long AmountInCents { get; }

    /// <summary>The ISO 4217 code the amount is in.</summary>
    public string Currency { get; }

    /// <summary>The date of the newest booking.</summary>
    public DateTime NewestBookedOn { get; }

    /// <summary>Whether the newest booking is a transfer between two of the owner's own accounts.</summary>
    public bool IsTransfer { get; }

    /// <summary>Whether the contract brings money in rather than costing it.</summary>
    public bool IsIncome => AmountInCents > 0;

    /// <summary>
    /// How many months one interval spans, or <see langword="null"/> where the
    /// interval is not one of the words a measured export carries.
    /// </summary>
    public int? MonthsPerPayment => MonthsPerInterval.TryGetValue(Interval, out var months) ? months : null;

    /// <summary>
    /// The amount spread over one month — a yearly contract contributes a
    /// twelfth — rounded to the cent, half away from zero. <see langword="null"/>
    /// where the interval is unknown, since nothing says how far to spread it.
    /// </summary>
    public long? MonthlyAmountInCents => MonthsPerPayment is { } months
        ? (long)Math.Round((decimal)AmountInCents / months, MidpointRounding.AwayFromZero)
        : null;

    /// <summary>
    /// Whether the newest booking lies more than
    /// <see cref="EndedAfterIntervals"/> intervals before
    /// <paramref name="currentTo"/>. The export carries no ended state, so this
    /// is a guess from the bookings stopping; a contract with an unknown
    /// interval is never guessed ended.
    /// </summary>
    /// <param name="currentTo">The date the export is current to.</param>
    public bool IsPresumablyEnded(DateOnly currentTo)
        => MonthsPerPayment is { } months
            && DateOnly.FromDateTime(NewestBookedOn).AddMonths(months * EndedAfterIntervals) < currentTo;

    /// <summary>
    /// The day the next booking is expected: the newest booking plus one
    /// interval in calendar months, so 31 January plus a month is the last day
    /// of February. <see langword="null"/> where the interval is unknown or the
    /// contract is presumably ended, since then nothing is expected.
    /// </summary>
    /// <param name="currentTo">The date the export is current to.</param>
    public DateOnly? NextBookingOn(DateOnly currentTo)
        => MonthsPerPayment is { } months && !IsPresumablyEnded(currentTo)
            ? DateOnly.FromDateTime(NewestBookedOn).AddMonths(months)
            : null;

    /// <summary>
    /// How many days the next booking is past its expected date, once more
    /// than <see cref="OverdueToleranceInDays"/> have passed without it;
    /// otherwise <see langword="null"/>. A booking that came would have become
    /// the newest one and moved the expected date on, so a date in the past
    /// means it has not come.
    /// </summary>
    /// <param name="currentTo">The date the export is current to.</param>
    public int? DaysOverdue(DateOnly currentTo)
        => NextBookingOn(currentTo) is { } expected && expected.AddDays(OverdueToleranceInDays) < currentTo
            ? currentTo.DayNumber - expected.DayNumber
            : null;
}
