namespace CashPrism.DemoData.Generation;

/// <summary>
/// A payment that repeats at a fixed interval — a contract, a standing order,
/// a fee — and the rows it leaves over the period a plan covers.
/// </summary>
public sealed record RecurringPayment
{
    /// <summary>
    /// The payment's name. It is what the contract identifier is derived from,
    /// so it has to be unique among the payments of a plan.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>The household account the payment is booked on.</summary>
    public required DemoAccount Account { get; init; }

    /// <summary>The other party.</summary>
    public required DemoParty Party { get; init; }

    /// <summary>The day of the month it is booked on; a shorter month takes its last day.</summary>
    public required int Day { get; init; }

    /// <summary>How often it repeats.</summary>
    public required DemoInterval Interval { get; init; }

    /// <summary>The signed amount of the first year.</summary>
    public required long AmountInCents { get; init; }

    /// <summary>The category.</summary>
    public required DemoCategory Category { get; init; }

    /// <summary>How it is paid.</summary>
    public required string TransactionKind { get; init; }

    /// <summary>The payment reference of the booking on a given day.</summary>
    public required Func<DateOnly, string> Reference { get; init; }

    /// <summary>Whether Finanzguru recognised it as a contract.</summary>
    public bool IsContract { get; init; } = true;

    /// <summary>How much the amount grows each calendar year after the first, in the direction of its sign.</summary>
    public long YearlyIncreaseInCents { get; init; }

    /// <summary>The index of the first month of the plan it is booked in.</summary>
    public int FirstMonth { get; init; }

    /// <summary>The last day it may be booked on, for a contract that ends; <see langword="null"/> runs to the end.</summary>
    public DateOnly? LastDay { get; init; }

    /// <summary>
    /// The household account the money goes to when the payment is a transfer
    /// between two of the household's own accounts; <see langword="null"/>
    /// otherwise.
    /// </summary>
    public DemoAccount? TransferTo { get; init; }

    /// <summary>The category of the incoming row of a transfer.</summary>
    public DemoCategory? IncomingCategory { get; init; }

    /// <summary>Whether the payment is left out of the freely disposable income.</summary>
    public bool IsExcludedFromDisposableIncome { get; init; }

    /// <summary>The rows the payment leaves over the period <paramref name="plan"/> covers.</summary>
    public IReadOnlyList<DemoBooking> Book(DemoPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var bookings = new List<DemoBooking>();
        var months = plan.Months();
        var contract = IsContract ? new DemoContract(DemoIds.ContractId(Name), Interval) : null;

        for (var index = FirstMonth; index < months.Count; index += Interval.Months)
        {
            var day = DemoPlan.On(months[index], Day);

            if (!plan.Covers(day) || day > LastDay)
            {
                continue;
            }

            var amount = AmountInCents
                + (Math.Sign(AmountInCents) * YearlyIncreaseInCents * (day.Year - plan.From.Year));
            var date = day.ToDateTime(TimeOnly.MinValue);

            if (TransferTo is null)
            {
                bookings.Add(plan.Book(Account, date, amount, Party, Category, TransactionKind) with
                {
                    PaymentReference = Reference(day),
                    Contract = contract,
                    IsExcludedFromDisposableIncome = IsExcludedFromDisposableIncome,
                });
            }
            else
            {
                bookings.AddRange(plan.Transfer(new DemoTransfer
                {
                    From = Account,
                    To = TransferTo,
                    Date = date,
                    AmountInCents = -amount,
                    OutgoingKind = TransactionKind,
                    OutgoingCategory = Category,
                    IncomingCategory = IncomingCategory ?? Category,
                    Payee = Party,
                    PaymentReference = Reference(day),
                    Contract = contract,
                    IsExcludedFromDisposableIncome = IsExcludedFromDisposableIncome,
                }));
            }
        }

        return bookings;
    }
}
