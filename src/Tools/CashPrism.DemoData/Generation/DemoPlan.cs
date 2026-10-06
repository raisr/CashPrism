namespace CashPrism.DemoData.Generation;

/// <summary>
/// The state one generation run shares between its planners: the period it
/// covers, the random number generator and the running count that numbers the
/// bookings. A fixed seed and a fixed order of calls are what make a run
/// repeatable.
/// </summary>
public sealed class DemoPlan
{
    private int _bookingCount;

    /// <summary>Starts a plan for the three years that end on <paramref name="until"/>.</summary>
    /// <param name="until">The last day the export covers.</param>
    /// <param name="seed">The seed of the random number generator.</param>
    public DemoPlan(DateOnly until, int seed)
    {
        Until = until;
        From = until.AddYears(-3).AddDays(1);
        Random = new Random(seed);
    }

    /// <summary>The first day the export covers.</summary>
    public DateOnly From { get; }

    /// <summary>The last day the export covers.</summary>
    public DateOnly Until { get; }

    /// <summary>The random number generator every planner draws from.</summary>
    public Random Random { get; }

    /// <summary>The first day of every month the plan touches, oldest first.</summary>
    public IReadOnlyList<DateOnly> Months()
    {
        var months = new List<DateOnly>();

        for (var month = new DateOnly(From.Year, From.Month, 1); month <= Until; month = month.AddMonths(1))
        {
            months.Add(month);
        }

        return months;
    }

    /// <summary>Every day the plan covers, oldest first.</summary>
    public IEnumerable<DateOnly> Days()
    {
        for (var day = From; day <= Until; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    /// <summary>Whether <paramref name="date"/> falls inside the period the export covers.</summary>
    public bool Covers(DateOnly date) => date >= From && date <= Until;

    /// <summary>
    /// The given <paramref name="day"/> of <paramref name="month"/>, or the
    /// month's last day where it is shorter.
    /// </summary>
    public static DateOnly On(DateOnly month, int day)
        => new(month.Year, month.Month, Math.Min(day, DateTime.DaysInMonth(month.Year, month.Month)));

    /// <summary>A random amount between <paramref name="minInCents"/> and <paramref name="maxInCents"/>, both included.</summary>
    public long Cents(long minInCents, long maxInCents) => Random.NextInt64(minInCents, maxInCents + 1);

    /// <summary>Whether an event of the given probability happens this time.</summary>
    public bool Chance(double probability) => Random.NextDouble() < probability;

    /// <summary>Picks one of <paramref name="items"/> at random.</summary>
    public T Pick<T>(IReadOnlyList<T> items) => items[Random.Next(items.Count)];

    /// <summary>A random time of day between 07:00 and 21:59, whole seconds.</summary>
    public TimeSpan TimeOfDay() => TimeSpan.FromSeconds(Random.Next(7 * 3600, 22 * 3600));

    /// <summary>
    /// Starts a booking and numbers it. Everything the booking carries beyond
    /// the columns every row has is added with <c>with</c>.
    /// </summary>
    public DemoBooking Book(
        DemoAccount account,
        DateTime date,
        long amountInCents,
        DemoParty party,
        DemoCategory category,
        string transactionKind)
        => new()
        {
            BookingId = DemoIds.BookingId(++_bookingCount),
            Account = account,
            Date = date,
            AmountInCents = amountInCents,
            Party = party,
            Category = category,
            TransactionKind = transactionKind,
        };

    /// <summary>
    /// The two rows a transfer between two of the household's accounts leaves:
    /// the outgoing one on <paramref name="from"/> and the incoming one on
    /// <paramref name="to"/>, both flagged as a transfer.
    /// </summary>
    public IReadOnlyList<DemoBooking> Transfer(DemoTransfer transfer)
    {
        ArgumentNullException.ThrowIfNull(transfer);

        var outgoing = Book(
                transfer.From,
                transfer.Date,
                -transfer.AmountInCents,
                transfer.Payee ?? DemoParty.Own(transfer.To),
                transfer.OutgoingCategory,
                transfer.OutgoingKind)
            with
        {
            PaymentReference = transfer.PaymentReference,
            Contract = transfer.Contract,
            IsInternalTransfer = true,
            IsExcludedFromDisposableIncome = transfer.IsExcludedFromDisposableIncome,
        };

        var incoming = Book(
                transfer.To,
                transfer.Date,
                transfer.AmountInCents,
                DemoParty.Own(transfer.From),
                transfer.IncomingCategory,
                transfer.IncomingKind)
            with
        {
            PaymentReference = transfer.PaymentReference,
            IsInternalTransfer = true,
            IsExcludedFromDisposableIncome = transfer.IsExcludedFromDisposableIncome,
        };

        return [outgoing, incoming];
    }
}
