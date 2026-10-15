using CashPrism.Application.Bookings;
using CashPrism.Application.CashFlow;

namespace CashPrism.Application.Accounts;

/// <summary>
/// Reads the <see cref="AccountFlows"/> of one account. Unlike the overview
/// and the analysis, it leaves transfers in: between two of the owner's own
/// accounts they move no money in total, but they do move this account's. Each
/// booking is income or spending by its own sign, and a split booking's money
/// is counted once, as the booking list sums it.
/// </summary>
/// <remarks>
/// Everything is counted back from the date the stored data is current to,
/// not from today, for the reason <see cref="CashFlowOverviewReader"/> gives.
/// </remarks>
public sealed class AccountFlowReader
{
    /// <summary>How many complete months the flows cover.</summary>
    public const int MonthsShown = 6;

    /// <summary>How many days, the last one included, count as lately.</summary>
    public const int RecentDays = 30;

    /// <summary>How many of the recent bookings are handed out.</summary>
    public const int RecentBookingsShown = 4;

    private readonly IBookingReader bookings;
    private readonly ICashFlowReader cashFlow;

    /// <summary>Creates the use case.</summary>
    /// <param name="bookings">Reads and sums the stored bookings.</param>
    /// <param name="cashFlow">Tells the date the stored data is current to.</param>
    public AccountFlowReader(IBookingReader bookings, ICashFlowReader cashFlow)
    {
        ArgumentNullException.ThrowIfNull(bookings);
        ArgumentNullException.ThrowIfNull(cashFlow);

        this.bookings = bookings;
        this.cashFlow = cashFlow;
    }

    /// <summary>
    /// Reads the flows of the account, or <see langword="null"/> when nothing
    /// was imported.
    /// </summary>
    /// <param name="accountReference">The account, by the reference the export carries.</param>
    /// <param name="cancellationToken">Cancels the queries.</param>
    public async Task<AccountFlows?> ReadAsync(string accountReference, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountReference);

        if (await cashFlow.ReadCurrentToAsync(cancellationToken) is not { } currentTo)
        {
            return null;
        }

        var lastMonth = new DateOnly(currentTo.Year, currentTo.Month, 1).AddMonths(-1);
        var firstMonth = lastMonth.AddMonths(1 - MonthsShown);
        var account = new BookingFilter(AccountReference: accountReference);

        var flows = (await bookings.ReadMonthlyFlowsAsync(account, firstMonth, lastMonth, cancellationToken))
            .ToDictionary(flow => flow.Month);
        var months = Enumerable.Range(0, MonthsShown)
            .Select(offset => firstMonth.AddMonths(offset))
            .Select(month => flows.GetValueOrDefault(month) ?? new MonthlyCashFlow(month, 0, 0))
            .ToList();

        var recent = new BookingPeriod(currentTo.AddDays(1 - RecentDays), currentTo);
        var lately = account with { Period = recent };
        var summary = await bookings.ReadSummaryAsync(lately, cancellationToken);
        var latest = await bookings.ReadPageAsync(
            new BookingPageRequest(Skip: 0, Take: RecentBookingsShown, Filter: lately),
            cancellationToken);

        return new AccountFlows(months, recent, summary, latest.Items);
    }
}
